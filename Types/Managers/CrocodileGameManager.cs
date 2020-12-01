using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Entities;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Managers.MessagesSender;
using ChapubelichBot.Types.Statics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = ChapubelichBot.Types.Entities.User;

namespace ChapubelichBot.Types.Managers
{
    class CrocodileGameManager
    {
        public static string Name => "\U0001F40AКрокодил\U0001F40A";

        // Fields
        private static ITelegramBotClient Client => ChapubelichClient.GetClient();
        private static Timer _deadSessionsCollector;

        // C-tor
        public static void Init()
        {
            int periodToCollect = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
            _deadSessionsCollector = new Timer(async _ => await CollectDeadSessionsAsync(), null, periodToCollect, periodToCollect);
        }

        public static async Task StartRequestAsync(Message message)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            CrocodileGameSession gameSession = await GetGameSessionOrNullAsync(message.Chat.Id, dbContext);
            if (gameSession == null)
            {
                gameSession = new CrocodileGameSession
                {
                    ChatId = message.Chat.Id,
                    HostCandidates = new LinkedList<User>(),
                    LastActivity = DateTime.UtcNow,
                    Word = await GetRandomWord(@"./Resources/crocodile/Words.txt")
                };

                await dbContext.CrocodileGameSessions.AddAsync(gameSession);
                try
                {
                    await dbContext.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    Console.WriteLine("Повторное добавление сессии крокодила");
                    return;
                }
            }
            else
            {
                Task sendingMessage = Client.TrySendTextMessageAsync(message.Chat.Id,
                    "Игра уже запущена!",
                    replyToMessageId: gameSession.GameMessageId);
                UpdateLastActivity(gameSession);
                await dbContext.SaveChangesAsync();
                await sendingMessage;
                return;
            }

            int replyId = message.From.Id == Client.BotId ? 0 : message.MessageId;

            var gameMessage = await Client.TrySendTextMessageAsync(message.Chat.Id,
                "Игра \"Крокодил\"!" +
                "\nНажмите на кнопку чтобы участвовать в выборе ведущего!",
                replyToMessageId: replyId,
                replyMarkup: InlineKeyboards.CrocodileRegistration);

            if (gameMessage != null)
            {
                gameSession.GameMessageId = gameMessage.MessageId;
                await dbContext.SaveChangesAsync();
            }
        }
        public static async Task AddToHostCandidatesRequestAsync(CallbackQuery callbackQuery)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();

            var gameSession = await GetGameSessionOrNullAsync(callbackQuery.Message.Chat.Id, dbContext);
            if (gameSession == null)
                return;

            UpdateLastActivity(gameSession);
            await dbContext.SaveChangesAsync();

            User user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == callbackQuery.From.Id);
            if (user == null)
                return;

            string answerMessage;

            if (gameSession.HostCandidates.All(u => u.UserId != user.UserId))
            {
                gameSession.HostCandidates.Add(user);
                await dbContext.SaveChangesAsync();
                await Client.TryAnswerCallbackQueryAsync(callbackQuery.Id);
                answerMessage = "Ты успешно добавлен в список кандидатов на ведущего! ✅";
            }
            else
                answerMessage = "Ты уже кандидат на ведущего!";

            await Client.TryAnswerCallbackQueryAsync(callbackQuery.Id, answerMessage);
        }


        public static async Task<CrocodileGameSession> GetGameSessionOrNullAsync(long chatId, ChapubelichdbContext dbContext)
        {
            CrocodileGameSession gameSession =
                await dbContext.CrocodileGameSessions
                    .Include(gs => gs.HostCandidates)
                    .Include(gs => gs.Host)
                    .FirstOrDefaultAsync(x => x.ChatId == chatId);
            return gameSession;
        }

        public static void Terminate()
        {
            _deadSessionsCollector.Dispose();
        }
        private static async Task CollectDeadSessionsAsync()
        {
            List<CrocodileGameSession> deadSessions;
            await using (var dbContext = new ChapubelichdbContext())
            {
                int timeToSessionDispose = ChapubelichClient.GetConfig().GetValue<int>("AppSettings:StopGameDelay");

                deadSessions = (await dbContext.CrocodileGameSessions
                    .Where(gs => gs.LastActivity < DateTime.UtcNow)
                    .ToListAsync())
                    .Where(gs => gs.LastActivity.AddSeconds(timeToSessionDispose) < DateTime.UtcNow)
                    .ToList();
            }

            Parallel.ForEach(deadSessions, async gs =>
            {
                await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
                gs = await GetGameSessionOrNullAsync(gs.ChatId, dbContext);

                Task deletingMessage = null;
                if (gs.GameMessageId != 0)
                    deletingMessage = Client.TryDeleteMessageAsync(gs.ChatId, gs.GameMessageId);
                Task sendingMessage = Client.TrySendTextMessageAsync(
                    gs.ChatId,
                    "Игровая сессия крокодила отменена из-за отсутствия активности",
                    replyMarkup: InlineKeyboards.RoulettePlayAgainMarkup);

                dbContext.CrocodileGameSessions.Remove(gs);
                try
                {
                    await dbContext.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    return;
                }

                if (deletingMessage != null)
                    await deletingMessage;

                await sendingMessage;
            });
        }

        private static void UpdateLastActivity(CrocodileGameSession gameSession)
        {
            gameSession.LastActivity = DateTime.UtcNow;
        }
        private static async Task<string> GetRandomWord(string pathOfWordsFile)
        {
            string[] words = await System.IO.File.ReadAllLinesAsync(pathOfWordsFile);
            return words[new Random().Next(words.Length)];
        }
    }
}
