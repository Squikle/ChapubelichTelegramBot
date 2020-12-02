using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private static Timer _startGameTimer;

        // C-tor
        public static void Init()
        {
            int periodToCollect = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
            _deadSessionsCollector = new Timer(async _ => await CollectDeadSessionsAsync(), null, periodToCollect, periodToCollect);

            int periodToStartGame = (int) TimeSpan.FromSeconds(5).TotalMilliseconds;
            _startGameTimer = new Timer(async _ => await StartGamesByTimer(), null, periodToStartGame, periodToStartGame);
        }
        public static void Terminate()
        {
            _deadSessionsCollector.Dispose();
            _startGameTimer.Dispose();
        }

        public static async Task CreateRequestAsync(Message message)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            CrocodileGameSession gameSession = await GetGameSessionOrNullAsync(message.Chat.Id, dbContext);
            if (gameSession == null)
            {
                Group group = await dbContext.Groups.FirstOrDefaultAsync(g => g.GroupId == message.Chat.Id);
                if (group == null)
                    return;
                gameSession = new CrocodileGameSession
                {
                    Group = group
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
                "Игра \"Крокодил\"🐊" +
                "\nНажми на кнопку чтобы участвовать в выборе ведущего!",
                replyToMessageId: replyId,
                replyMarkup: InlineKeyboards.CrocodileRegistrationMarkup);

            if (gameMessage != null)
            {
                gameSession.GameMessageId = gameMessage.MessageId;
                gameSession.GameMessageText = gameMessage.Text;
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

            CrocodileGameSession alreadyHostingGameSession = await dbContext.CrocodileGameSessions
                .Include(gs => gs.HostCandidates)
                .Include(gs => gs.Host)
                .Include(gs => gs.Group)
                .FirstOrDefaultAsync(
                    gs => gs.HostCandidates
                        .Any(c => c.CandidateId == user.UserId) || gs.Host.UserId == callbackQuery.From.Id);
            if (alreadyHostingGameSession != null)
            {
                answerMessage = alreadyHostingGameSession.Group.GroupId != callbackQuery.Message.Chat.Id 
                    ? "Ты не можешь быть ведущим в нескольких чатах сразу!" : "Ты уже являешься кандидатом на ведущего!";
            }
            else
            {
                string candidateName = string.Empty;
                ChatMember candidate = await Client.GetChatMemberAsync(gameSession.Group.GroupId, callbackQuery.From.Id);
                if (candidate != null)
                    candidateName = candidate.User.FirstName;

                gameSession.HostCandidates.Add(new CrocodileHostCandidate {Candidate = user});
                try
                {
                    await dbContext.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    Console.WriteLine("Повторное добавление юзера в кандидатов на хостинг крокодила");
                    return;
                }

                await Client.TryAnswerCallbackQueryAsync(callbackQuery.Id);
                answerMessage = "Ты успешно добавлен в список кандидатов на ведущего! ✅";
                if (!string.IsNullOrEmpty(candidateName))
                {
                    string newGameMessageText = gameSession.GameMessageText;
                    if (gameSession.HostCandidates.Count > 1)
                        newGameMessageText += "\nСписок желающих быть ведущим:";
                    newGameMessageText += $"\n<b>{gameSession.HostCandidates.Count}.</b> <i><a href=\"tg://user?id={callbackQuery.From.Id}\">{candidateName}</a></i>";
                    await Client.TryEditMessageAsync(gameSession.Group.GroupId, gameSession.GameMessageId,
                        newGameMessageText, ParseMode.Html,
                    replyMarkup: InlineKeyboards.CrocodileRegistrationMarkup);

                    gameSession.GameMessageText = newGameMessageText;
                    await dbContext.SaveChangesAsync();
                } 
            }

            await Client.TryAnswerCallbackQueryAsync(callbackQuery.Id, answerMessage);
        }

        public static async Task ChooseWordRequestTask(CallbackQuery callbackQuery)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            CrocodileGameSession gameSession =
                await dbContext.CrocodileGameSessions
                    .Include(gs => gs.Host)
                    .FirstOrDefaultAsync(gs => gs.Host.UserId == callbackQuery.From.Id);
            if (gameSession == null || !string.IsNullOrEmpty(gameSession.GameWord) || !gameSession.Started)
                return;

            UpdateLastActivity(gameSession);
            await dbContext.SaveChangesAsync();

            string choosenWord = string.Empty;
            switch (callbackQuery.Data)
            {
                case "crocodileChooseFirstWord":
                    choosenWord = gameSession.WordVariants[0];
                    break;
                case "crocodileChooseSecondWord":
                    choosenWord = gameSession.WordVariants[1];
                    break;
                case "crocodileChooseThirdWord":
                    choosenWord = gameSession.WordVariants[2];
                    break;
            }

            gameSession.GameWord = choosenWord;
            await dbContext.SaveChangesAsync();
            Task deletingCallbackMessage =
                Client.TryDeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
            Task sendingPrivateMessage = Client.TrySendTextMessageAsync(callbackQuery.Message.Chat.Id, 
                $"Ты выбрал слово \"<i>{choosenWord}</i>\"" +
                "\nТеперь помоги другим игрокам отгадать его!",
                ParseMode.Html);
            Message newGameMessage = await Client.TrySendTextMessageAsync(gameSession.GroupId, "<b>Игра началась!</b>" +
                $"\nЗагаданное слово отправлено в личные сообщения ведущему <i>{callbackQuery.From.FirstName}</i>" +
                "\n👑<i>Ведущий</i> должен объяснить загаданное слово не используя однокоренные слова" +
                "\n👤<i>Остальные</i> учасники должны отгадать что это за слово как можно быстрее",
                ParseMode.Html);
            gameSession.GameMessageId = newGameMessage?.MessageId ?? 0;
            await dbContext.SaveChangesAsync();
            await sendingPrivateMessage;
            await deletingCallbackMessage;
        }

        private static async Task StartGameSessionAsync(CrocodileGameSession gameSession)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            gameSession = await GetGameSessionOrNullAsync(gameSession.Group.GroupId, dbContext);
            if (gameSession == null)
                return;

            if (gameSession.Started)
                return;

            UpdateLastActivity(gameSession);
            await dbContext.SaveChangesAsync();

            if (gameSession.WordVariants == null)
            {
                string[] wordVariants = await GetRandomWordsAsync(@"./Resources/crocodile/Words.txt", 3);

                gameSession.WordVariants = wordVariants;
                await dbContext.SaveChangesAsync();
            }

            if (gameSession.Host == null)
            {
                HashSet<int> tryedUsers = new HashSet<int>();
                Message wordChooseMessage = null;
                Random rand = new Random();
                User host = null;
                while (wordChooseMessage == null && tryedUsers.Count < gameSession.HostCandidates.Count)
                {
                    host = gameSession.HostCandidates[rand.Next(gameSession.HostCandidates.Count)].Candidate;
                    if (!tryedUsers.Contains(host.UserId))
                        tryedUsers.Add(host.UserId);
                    else continue;
                    wordChooseMessage = await Client.TrySendTextMessageAsync(host.UserId,
                        $"Ты выбран в качестве ведущего в группе <i>{gameSession.Group.Name}</i>. Выбери одно из 3 предложенных слов:",
                        replyMarkup: InlineKeyboards.GetCrocodileChooseWordMarkup(gameSession.WordVariants[0], gameSession.WordVariants[1], gameSession.WordVariants[2]),
                        parseMode: ParseMode.Html);
                }
                if (wordChooseMessage == null && tryedUsers.Count == gameSession.HostCandidates.Count || host == null)
                {
                    if (await DeleteGameSessionAsync(gameSession, dbContext))
                        await Client.TrySendTextMessageAsync(gameSession.Group.GroupId,
                            "Не могу отправить сообщение <i>Ведущему</i>. Игра отменена 😞",
                            ParseMode.Html);
                    return;
                }

                
                ChatMember hostMember = await Client.GetChatMemberAsync(gameSession.Group.GroupId, host.UserId);
                if (hostMember == null)
                    return;

                string choosenHostText = "<b>Ведущий выбран!</b>\n" +
                                         $"Загаданное слово отправлено в личные сообщения Ведущему <i><a href=\"tg://user?id={hostMember.User.Id}\">{hostMember.User.FirstName}</a></i>";
                Message newGameMessage = await Client.TrySendTextMessageAsync(gameSession.Group.GroupId,
                    choosenHostText,
                    ParseMode.Html);

                await Client.TryEditMessageReplyMarkupAsync(gameSession.Group.GroupId, gameSession.GameMessageId);

                dbContext.RemoveRange(gameSession.HostCandidates);
                gameSession.Host = host;
                gameSession.GameMessageId = newGameMessage?.MessageId ?? 0;
                gameSession.GameMessageText = null;
                gameSession.Started = true;
                await dbContext.SaveChangesAsync();
            }
        }
        private static async Task<CrocodileGameSession> GetGameSessionOrNullAsync(long chatId, ChapubelichdbContext dbContext)
        {
            CrocodileGameSession gameSession =
                await dbContext.CrocodileGameSessions
                    .Include(gs => gs.HostCandidates)
                    .ThenInclude(hc => hc.Candidate)
                    .Include(gs => gs.Host)
                    .Include(gs => gs.Group)
                    .FirstOrDefaultAsync(x => x.Group.GroupId == chatId);
            return gameSession;
        }

        private static async Task CollectDeadSessionsAsync()
        {
            List<CrocodileGameSession> deadSessions;
            await using (var dbContext = new ChapubelichdbContext())
            {
                int timeToSessionDispose = ChapubelichClient.GetConfig().GetValue<int>("AppSettings:StopGameDelay");

                deadSessions = (await dbContext.CrocodileGameSessions
                        .Include(gs => gs.Group)
                        .Where(gs => gs.LastActivity < DateTime.UtcNow)
                        .ToListAsync())
                        .Where(gs => gs.LastActivity.AddSeconds(timeToSessionDispose) < DateTime.UtcNow)
                        .ToList();
            }

            Parallel.ForEach(deadSessions, async gs =>
            {
                await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
                gs = await GetGameSessionOrNullAsync(gs.Group.GroupId, dbContext);

                if (await DeleteGameSessionAsync(gs, dbContext))
                    await Client.TrySendTextMessageAsync(
                        gs.Group.GroupId,
                        "Игровая сессия крокодила отменена из-за отсутствия активности",
                        //TODO: перезапускать крокодила, а не рулетку
                        replyMarkup: InlineKeyboards.CrocodilePlayAgainMarkup);
            });
        }

        private static async Task<bool> DeleteGameSessionAsync(CrocodileGameSession gameSession, ChapubelichdbContext dbContext)
        {
            if (gameSession == null)
                return false;

            Task deletingMessage = null;
            if (gameSession.GameMessageId != 0)
                deletingMessage = Client.TryDeleteMessageAsync(gameSession.Group.GroupId, gameSession.GameMessageId);
            dbContext.CrocodileGameSessions.Remove(gameSession);
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return false;
            }

            if (deletingMessage != null)
                await deletingMessage;

            return true;
        }
        private static void UpdateLastActivity(CrocodileGameSession gameSession)
        {
            gameSession.LastActivity = DateTime.UtcNow;
        }
        private static async Task<string[]> GetRandomWordsAsync(string pathOfWordsFile, int count)
        {
            Random rand = new Random();
            string[] words = await System.IO.File.ReadAllLinesAsync(pathOfWordsFile);
            string[] selectedWords = new string[count];
            for (int i = 0; i < selectedWords.Length; i++)
                selectedWords[i] = words[rand.Next(words.Length)];
            return selectedWords;
        }
        private static async Task StartGamesByTimer()
        {
            int secondsToStartGame = 5;
            List<CrocodileGameSession> gameSessions;
            await using (ChapubelichdbContext dbContext = new ChapubelichdbContext())
                gameSessions = dbContext.CrocodileGameSessions
                    .Include(gs => gs.HostCandidates)
                    .Include(gs => gs.Group)
                    // TODO: вернуть "> 1" после тестов 
                    .Where(gs => !gs.Started && gs.HostCandidates.Count > 0)
                    .ToList();
            Parallel.ForEach(gameSessions, async gs =>
            {
                if (gs.HostCandidates.Count > 0)
                {
                    if (gs.HostCandidates[^1].RegistrationTime.AddSeconds(secondsToStartGame) <
                        DateTime.UtcNow)
                        await StartGameSessionAsync(gs);
                }
            });
        }
    }
}
