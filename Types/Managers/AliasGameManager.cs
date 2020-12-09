using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Entities.Alias;
using ChapubelichBot.Types.Managers.MessagesSender;
using ChapubelichBot.Types.Statics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Group = ChapubelichBot.Types.Entities.Groups.Group;
using User = ChapubelichBot.Types.Entities.Users.User;

namespace ChapubelichBot.Types.Managers
{
    class AliasGameManager
    {
        public static string Name => "\U0001F64AАлиас\U0001F64A";

        // Fields
        private static ITelegramBotClient Client => ChapubelichClient.GetClient();
        private static Timer _deadSessionsCollector;
        private static Timer _startGameTimer;

        // C-tor
        public static void Init()
        {
            int periodToCollect = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
            _deadSessionsCollector = new Timer(async _ => await CollectDeadSessionsAsync(), null, periodToCollect, periodToCollect);

            int periodToStartGame = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;
            _startGameTimer = new Timer(async _ => await StartGamesByTimerAsync(), null, periodToStartGame, periodToStartGame);
        }
        public static void Terminate()
        {
            _deadSessionsCollector.Dispose();
            _startGameTimer.Dispose();
        }

        public static async Task CreateRequestAsync(Message message)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            AliasGameSession gameSession = await GetGameSessionOrNullAsync(message.Chat.Id, dbContext);
            if (gameSession == null)
            {
                Group group = await dbContext.Groups.FirstOrDefaultAsync(g => g.GroupId == message.Chat.Id);
                if (group == null)
                    return;
                gameSession = new AliasGameSession
                {
                    Group = group
                };

                await dbContext.AliasGameSessions.AddAsync(gameSession);
                try
                {
                    await dbContext.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    Console.WriteLine("Повторное добавление сессии алиаса");
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
                "Игра \"Алиас\"🙊" +
                "\nНажми на кнопку чтобы участвовать в выборе ведущего!",
                replyToMessageId: replyId,
                replyMarkup: InlineKeyboards.AliasRegistrationMarkup);

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

            AliasGameSession alreadyHostingGameSession = await dbContext.AliasGameSessions
                .Include(gs => gs.HostCandidates)
                .Include(gs => gs.Host)
                .Include(gs => gs.Group)
                .FirstOrDefaultAsync(
                    gs => gs.HostCandidates
                        .Any(c => c.CandidateId == user.UserId) || gs.Host.UserId == callbackQuery.From.Id);
            if (alreadyHostingGameSession != null)
            {
                string answerMessage = alreadyHostingGameSession.Group.GroupId != callbackQuery.Message.Chat.Id
                    ? "Ты не можешь быть ведущим в нескольких чатах сразу!"
                    : "Ты уже являешься участвуешь в выборе ведущего!";
                await Client.TryAnswerCallbackQueryAsync(callbackQuery.Id, answerMessage);
                return;
            }

            ChatMember candidate = await Client.GetChatMemberAsync(gameSession.Group.GroupId, callbackQuery.From.Id);
            if (candidate == null)
                return;
            var candidateName = candidate.User.FirstName;

            AliasHostCandidate newHostCandidate = new AliasHostCandidate {Candidate = user};
            gameSession.HostCandidates.Add(newHostCandidate);
            string newGameMessageText = gameSession.GameMessageText;
            if (gameSession.HostCandidates.Count == 1)
                newGameMessageText += "\nСписок желающих быть ведущим:";
            newGameMessageText += $"\n<b>{gameSession.HostCandidates.Count}.</b> <i><a href=\"tg://user?id={callbackQuery.From.Id}\">{candidateName}</a></i>";
            gameSession.GameMessageText = newGameMessageText;

            bool saved = false;
            while (!saved)
            {
                try
                {
                    await dbContext.SaveChangesAsync();
                    saved = true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries)
                        if (entry.Entity is AliasGameSession entryGameSession)
                        {
                            Console.WriteLine(
                                "Конфликт параллелизма для сохранения текста игрового сообщения алиаса");
                            await entry.ReloadAsync();

                            entryGameSession.HostCandidates.Add(newHostCandidate);
                            newGameMessageText = entryGameSession.GameMessageText;
                            if (gameSession.HostCandidates.Count == 1)
                                newGameMessageText += "\nСписок желающих быть ведущим:";
                            newGameMessageText += $"\n<b>{gameSession.HostCandidates.Count}.</b> <i><a href=\"tg://user?id={callbackQuery.From.Id}\">{candidateName}</a></i>";
                            entryGameSession.GameMessageText = newGameMessageText;
                        }
                }
                catch (DbUpdateException)
                {
                    Console.WriteLine("Повторное добавление юзера в кандидатов на хостинг алиаса");
                    return;
                }
            }

            await Client.TryEditMessageAsync(gameSession.Group.GroupId, gameSession.GameMessageId,
                newGameMessageText, ParseMode.Html,
                replyMarkup: InlineKeyboards.AliasRegistrationMarkup);
            await Client.TryAnswerCallbackQueryAsync(callbackQuery.Id, "Ты успешно добавлен в список возможных ведущих! ✅");

            int maxNumberHostingCandidates = ChapubelichClient.GetConfig()
                .GetValue<int>("AliasSettings:MaxNumberHostingCandidates");
            if (gameSession.HostCandidates.Count >= maxNumberHostingCandidates)
                await StartGameSessionAsync(gameSession);
        }

        public static async Task ChooseWordRequestAsync(CallbackQuery callbackQuery)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            AliasGameSession gameSession =
                await dbContext.AliasGameSessions
                    .Include(gs => gs.Host)
                    .FirstOrDefaultAsync(gs => gs.Host.UserId == callbackQuery.From.Id);
            if (gameSession == null || !string.IsNullOrEmpty(gameSession.GameWord) || gameSession.StartTime == null)
                return;

            UpdateLastActivity(gameSession);
            await dbContext.SaveChangesAsync();

            string choosenWord = string.Empty;
            switch (callbackQuery.Data)
            {
                case "aliasChooseFirstWord":
                    choosenWord = gameSession.WordVariants[0];
                    break;
                case "aliasChooseSecondWord":
                    choosenWord = gameSession.WordVariants[1];
                    break;
                case "aliasChooseThirdWord":
                    choosenWord = gameSession.WordVariants[2];
                    break;
            }

            gameSession.GameWord = choosenWord;
            gameSession.StartTime = DateTime.UtcNow;
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
                "\n👤<i>Остальные</i> учасники должны отгадать что это за слово как можно быстрее, чтобы получить награду",
                ParseMode.Html);
            gameSession.GameMessageId = newGameMessage?.MessageId ?? 0;
            await dbContext.SaveChangesAsync();
            await sendingPrivateMessage;
            await deletingCallbackMessage;
        }
        private static async Task StartGameSessionAsync(AliasGameSession gameSession)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            gameSession = await GetGameSessionOrNullAsync(gameSession.Group.GroupId, dbContext);
            if (gameSession == null)
                return;

            if (gameSession.StartTime != null)
                return;

            UpdateLastActivity(gameSession);
            await dbContext.SaveChangesAsync();

            if (gameSession.WordVariants == null)
            {
                string[] wordVariants = await GetRandomWordsAsync(@"./Resources/alias/Words.txt", 3);

                gameSession.WordVariants = wordVariants;
                await dbContext.SaveChangesAsync();
            }

            if (gameSession.Host == null)
            {
                HashSet<int> tryedUserIds = new HashSet<int>();
                Message wordChooseMessage = null;
                Random rand = new Random();
                User host = null;
                while (wordChooseMessage == null && tryedUserIds.Count < gameSession.HostCandidates.Count)
                {
                    host = gameSession.HostCandidates[rand.Next(gameSession.HostCandidates.Count)].Candidate;
                    if (tryedUserIds.Contains(host.UserId))
                        continue;

                    wordChooseMessage = await Client.TrySendTextMessageAsync(host.UserId,
                        $"Ты выбран в качестве ведущего в группе <i>{gameSession.Group.Name}</i>. Выбери одно из 3 предложенных слов:",
                        replyMarkup: InlineKeyboards.GetAliasChooseWordMarkup(gameSession.WordVariants[0], gameSession.WordVariants[1], gameSession.WordVariants[2]),
                        parseMode: ParseMode.Html);

                    tryedUserIds.Add(host.UserId);
                }
                if (wordChooseMessage == null && tryedUserIds.Count == gameSession.HostCandidates.Count || host == null)
                {
                    await DeleteGameSessionAsync(gameSession, dbContext);
                    try
                    {
                        await dbContext.SaveChangesAsync();
                    }
                    catch (DbUpdateException)
                    {
                        Console.WriteLine("Повторное удаление игровой сессии алиаса");
                        return;
                    }
                    await Client.TrySendTextMessageAsync(gameSession.Group.GroupId,
                        "Не удалось отправить сообщение ни одному из возможных <i>Ведущих</i>. Игра отменена 😞",
                        ParseMode.Html, replyMarkup: InlineKeyboards.AliasPlayAgainMarkup);
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
                gameSession.StartTime = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
            }
        }

        public static async Task GuessTheWordRequestAsync(Message message)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            AliasGameSession gameSession = await GetGameSessionOrNullAsync(message.Chat.Id, dbContext);
            if (gameSession == null || string.IsNullOrEmpty(gameSession.GameWord) || gameSession.StartTime == null)
                return;

            User guessingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == message.From.Id);
            if (guessingUser == null || guessingUser.UserId == gameSession.Host.UserId)
                return;

            UpdateLastActivity(gameSession);
            await dbContext.SaveChangesAsync();

            if (IsWordGuessCorrect(message.Text, gameSession.GameWord))
                await FinishGameAsync(gameSession, dbContext, guessingUser, message);
            else
                await AddAttemptsAndSaveAsync(gameSession, dbContext);
        }

        private static async Task<AliasGameSession> GetGameSessionOrNullAsync(long chatId, ChapubelichdbContext dbContext)
        {
            AliasGameSession gameSession =
                await dbContext.AliasGameSessions
                    .Include(gs => gs.HostCandidates)
                    .ThenInclude(hc => hc.Candidate)
                    .Include(gs => gs.Host)
                    .Include(gs => gs.Group)
                    .FirstOrDefaultAsync(x => x.Group.GroupId == chatId);
            return gameSession;
        }
        private static void UpdateLastActivity(AliasGameSession gameSession)
        {
            gameSession.LastActivity = DateTime.UtcNow;
        }
        private static async Task StartGamesByTimerAsync()
        {
            int secondsToStartGame = ChapubelichClient.GetConfig().GetValue<int>("AliasSettings:StartAliasGameDelaySeconds");

            List<AliasGameSession> gameSessions;
            await using (ChapubelichdbContext dbContext = new ChapubelichdbContext())
                gameSessions = dbContext.AliasGameSessions
                    .Include(gs => gs.HostCandidates)
                    .Include(gs => gs.Group)
                    .Where(gs => gs.StartTime == null && gs.HostCandidates.Count > 0)
                    .ToList();
            Parallel.ForEach(gameSessions, async gs =>
            {
                if (gs.HostCandidates[^1].RegistrationTime.AddSeconds(secondsToStartGame) <
                    DateTime.UtcNow)
                    await StartGameSessionAsync(gs);
            });
        }
        private static async Task CollectDeadSessionsAsync()
        {
            List<AliasGameSession> deadSessions;
            await using (var dbContext = new ChapubelichdbContext())
            {
                int timeToSessionDispose = ChapubelichClient.GetConfig().GetValue<int>("AliasSettings:StopAliasGameDelay");

                deadSessions = (await dbContext.AliasGameSessions
                        .Include(gs => gs.Group)
                        .Where(gs => gs.LastActivity < DateTime.UtcNow)
                        .ToListAsync())
                        .Where(gs => gs.LastActivity.AddSeconds(timeToSessionDispose) < DateTime.UtcNow)
                        .ToList();
            }

            Parallel.ForEach(deadSessions, async gs =>
            {
                if (gs == null)
                    return;

                await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
                dbContext.AliasGameSessions.Attach(gs);

                await DeleteGameSessionAsync(gs, dbContext);
                try
                {
                    await dbContext.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    Console.WriteLine("Повторное удаление игровой сессии алиаса");
                    return;
                }
                string message = "Игровая сессия <i>алиаса</i> отменена из-за отсутствия активности";
                if (!string.IsNullOrEmpty(gs.GameWord))
                    message += $"\nЭто было слово: <i>{gs.GameWord}</i>";
                await Client.TrySendTextMessageAsync(
                    gs.Group.GroupId,
                    message,
                    ParseMode.Html,
                    replyMarkup: InlineKeyboards.AliasPlayAgainMarkup);
            });
        }
        private static async Task DeleteGameSessionAsync(AliasGameSession gameSession, ChapubelichdbContext dbContext)
        {
            if (gameSession.GameMessageId != 0)
                await Client.TryDeleteMessageAsync(gameSession.Group.GroupId, gameSession.GameMessageId);

            dbContext.AliasGameSessions.Remove(gameSession);
        }
        
        private static async Task<string[]> GetRandomWordsAsync(string pathOfWordsFile, int count)
        {
            Random rand = new Random();
            string[] words = await System.IO.File.ReadAllLinesAsync(pathOfWordsFile);
            HashSet<string> selectedWords = new HashSet<string>(count);
            while (selectedWords.Count < count && selectedWords.Count < words.Length)
            {
                var pickedWord = words[rand.Next(words.Length)];
                if (!selectedWords.Contains(pickedWord))
                    selectedWords.Add(pickedWord);
            }
            return selectedWords.ToArray();
        }
        private static bool IsWordGuessCorrect(string guessWord, string gameSessionWord)
        {
            guessWord = Regex.Replace(guessWord, "э", "е", RegexOptions.IgnoreCase);
            guessWord = Regex.Replace(guessWord, "ё", "е", RegexOptions.IgnoreCase);

            string originWord = gameSessionWord;
            originWord = Regex.Replace(originWord, "э", "е", RegexOptions.IgnoreCase);
            originWord = Regex.Replace(originWord, "ё", "е", RegexOptions.IgnoreCase);

            var compareOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols;
            return String.Compare(guessWord, originWord, CultureInfo.InvariantCulture, compareOptions) == 0;
        }
        private static long GetPlayerReward(AliasGameSession gameSession)
        {
            if (!gameSession.StartTime.HasValue)
                return 0;

            IConfiguration config = ChapubelichClient.GetConfig();
            int maxSecondsToGetAliasReward = config.GetValue<int>("AliasSettings:MaxSecondsToGetAliasReward");
            TimeSpan elapsedTime = DateTime.UtcNow.Subtract(gameSession.StartTime.Value);

            if (elapsedTime.Seconds > maxSecondsToGetAliasReward)
                return 0;

            int maxReward = config.GetValue<int>("AliasSettings:MaxAliasReward");
            double maxSecondForReward = TimeSpan.FromSeconds(maxSecondsToGetAliasReward).TotalSeconds;
            double divider = maxSecondForReward / 100;
            return (long)(maxReward * (maxSecondForReward - elapsedTime.TotalSeconds) / divider * 0.01d) + 1;
        }

        private static async Task AddAttemptsAndSaveAsync(AliasGameSession gameSession, ChapubelichdbContext dbContext)
        {
            gameSession.Attempts++;

            bool saved = false;
            while (!saved)
            {
                try
                {
                    await dbContext.SaveChangesAsync();
                    saved = true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Console.WriteLine("Конфликт параллелизма для добавления попыток алиаса");
                    await ex.Entries.Single().ReloadAsync();
                }
            }
        }
        private static async Task FinishGameAsync(AliasGameSession gameSession, ChapubelichdbContext dbContext, User guessingUser, Message message)
        {
            long reward = GetPlayerReward(gameSession);

            await DeleteGameSessionAsync(gameSession, dbContext);

            if (reward > 0)
                guessingUser.Balance += reward;

            bool saved = false;
            while (!saved)
            {
                try
                {
                    await dbContext.SaveChangesAsync();
                    saved = true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries)
                    {
                        if (entry.Entity is User user)
                        {
                            Console.WriteLine("Конфликт параллелизма для баланса пользователя (AliasGameManager)");
                            await entry.ReloadAsync();

                            user.Balance += reward;
                        }
                    }
                }
                catch (DbUpdateException ex)
                {
                    foreach (var entry in ex.Entries)
                        Console.WriteLine(entry.Entity is AliasGameSession
                            ? "Повторное удаление игровой сессии алиаса"
                            : $"Ошибка сохраненния {entry.Entity.GetType()} (AliasGameManager)");
                    return;
                }
            }

            await AddAttemptsAndSaveAsync(gameSession, dbContext);

            string answer = "Правильно!" +
                            $"\nИгрок <i><a href=\"tg://user?id={message.From.Id}\">{message.From.FirstName}</a></i> разгадал слово \"<i>{gameSession.GameWord}</i>\"" +
                            $"{(reward > 0 ? $" и получил <b>{reward}</b> 💰!" : " но ничего не получил 😔")}" +
                            $"\nВсего было попыток: <b>{gameSession.Attempts}</b>";
            await Client.TrySendTextMessageAsync(gameSession.GroupId, answer,
                ParseMode.Html, replyToMessageId: message.MessageId,
                replyMarkup: InlineKeyboards.AliasPlayAgainMarkup);
        }
    }
}
