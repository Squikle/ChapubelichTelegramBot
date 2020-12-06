using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Entities;
using ChapubelichBot.Types.Enums;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Managers.MessagesSender;
using ChapubelichBot.Types.Statics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Group = ChapubelichBot.Types.Entities.Group;
using User = ChapubelichBot.Types.Entities.User;

namespace ChapubelichBot.Types.Managers
{
    public static class RouletteGameManager
    {
        public static string Name => "\U0001F525Рулетка\U0001F525";
        public const int TableSize = 37;
        // Fields
        private static ITelegramBotClient Client => ChapubelichClient.GetClient();
        private static Timer _deadSessionsCollector;

        // C-tor
        public static void Init()
        {
            int periodToCollect = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
            _deadSessionsCollector = new Timer(async _ => await CollectDeadSessionsAsync(), null, periodToCollect, periodToCollect);
        }
        public static void Terminate()
        {
            _deadSessionsCollector.Dispose();
        }

        // Public
        public static async Task CreateRequestAsync(Message message)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            RouletteGameSession gameSession = await GetGameSessionOrNullAsync(message.Chat.Id, dbContext);
            if (gameSession == null)
            {
                gameSession = new RouletteGameSession
                {
                    ChatId = message.Chat.Id,
                    Resulting = false,
                    ColorBetTokens = new List<RouletteColorBetToken>(),
                    NumberBetTokens = new List<RouletteNumbersBetToken>(),
                    LastActivity = DateTime.UtcNow,
                    ResultNumber = GetRandomResultNumber()
                };

                await dbContext.RouletteGameSessions.AddAsync(gameSession);
                try
                {
                    await dbContext.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    Console.WriteLine("Повторное добавление сессии рулетки");
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

            var gameMessage = await Client.TrySendPhotoAsync(gameSession.ChatId,
                "https://i.imgur.com/SN8DRoa.png",
                caption: "Игра запущена. Ждем ваши ставки...\n" +
                         "Ты можешь поставить ставку по умолчанию на предложенные ниже варианты:",
                replyToMessageId: replyId,
                replyMarkup: InlineKeyboards.RouletteBetsMarkup);

            if (gameMessage != null)
            {
                gameSession.GameMessageId = gameMessage.MessageId;
                await dbContext.SaveChangesAsync();
            }
        }
        public static async Task ResumeResultingAsync(long chatId)
        {
            Task<Chat> getChatTask = Client.GetChatAsync(chatId);

            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();

            RouletteGameSession gameSession = await GetGameSessionOrNullAsync(chatId, dbContext);
            if (gameSession == null)
                return;

            Chat chat = await getChatTask;

            if (chat != null)
            {
                await UpdateLogAsync(gameSession, dbContext, chat.Type);
                await dbContext.SaveChangesAsync();
            }

            // Удаление сообщений и отправка результатов
            string result = await SummarizeAsync(gameSession);

            dbContext.Remove(gameSession);
            try
            {
                Console.WriteLine("Повторное удаление сессии рулетки");
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return;
            }

            Task deletingAnimationMessage = null;
            if (gameSession.AnimationMessageId != 0)
                deletingAnimationMessage = Client.TryDeleteMessageAsync(gameSession.ChatId, gameSession.AnimationMessageId);

            Task deletingGameMessage = null;
            if (gameSession.AnimationMessageId != 0)
                deletingGameMessage = Client.TryDeleteMessageAsync(gameSession.ChatId, gameSession.GameMessageId);

            Task sendingResult = Client.TrySendTextMessageAsync(
                gameSession.ChatId,
                result,
                ParseMode.Html,
                replyMarkup: InlineKeyboards.RoulettePlayAgainMarkup);

            if (deletingAnimationMessage != null)
                await deletingAnimationMessage;
            if (deletingGameMessage != null)
                await deletingGameMessage;
            await sendingResult;
        }
        public static async Task BetCancelRequestAsync(CallbackQuery callbackQuery)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();

            var gameSession = await GetGameSessionOrNullAsync(callbackQuery.Message.Chat.Id, dbContext);
            if (gameSession == null || gameSession.Resulting)
                return;

            UpdateLastActivity(gameSession);
            await dbContext.SaveChangesAsync();

            User user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserId == callbackQuery.From.Id);
            if (user == null)
                return;

            string answerMessage = await CancelBetAsync(gameSession, user, callbackQuery.From.FirstName, dbContext);
            await dbContext.SaveChangesAsync();

            Task sendingMessage = Client.TrySendTextMessageAsync(
                gameSession.ChatId,
                answerMessage,
                parseMode: ParseMode.Html);

            await Client.TryAnswerCallbackQueryAsync(callbackQuery.Id);
            await sendingMessage;
        }
        public static async Task BetCancelRequestAsync(Message message)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();

            var gameSession = await GetGameSessionOrNullAsync(message.Chat.Id, dbContext);
            if (gameSession == null || gameSession.Resulting)
                return;

            UpdateLastActivity(gameSession);
            await dbContext.SaveChangesAsync();

            User user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserId == message.From.Id);
            if (user == null)
                return;

            string answerMessage = await CancelBetAsync(gameSession, user, message.From.FirstName, dbContext);
            await dbContext.SaveChangesAsync();

            await Client.TrySendTextMessageAsync(
                gameSession.ChatId,
                answerMessage,
                replyToMessageId: message.MessageId,
                parseMode: ParseMode.Html);
        }
        public static async Task BetColorRequestAsync(CallbackQuery callbackQuery)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();

            var gameSession = await GetGameSessionOrNullAsync(callbackQuery.Message.Chat.Id, dbContext);
            if (gameSession == null || gameSession.Resulting)
                return;

            UpdateLastActivity(gameSession);
            await dbContext.SaveChangesAsync();

            User user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserId == callbackQuery.From.Id);

            if (user == null)
                return;

            if (user.Balance == 0)
            {
                await Client.TryAnswerCallbackQueryAsync(callbackQuery.Id,
                    "Ты не можешь сделать ставку. У тебя нет денег 😞");
                return;
            }

            string allInAlertMessage = null;
            long playerBetSum = user.DefaultBet;
            if (playerBetSum >= user.Balance)
            {
                allInAlertMessage = "Ты ставишь все свои средства!";
                playerBetSum = user.Balance;
            }

            // Определение ставки игрока
            RouletteColorEnum playerChoose;

            switch (callbackQuery.Data)
            {
                case "rouletteBetRed":
                    playerChoose = RouletteColorEnum.Red;
                    break;
                case "rouletteBetBlack":
                    playerChoose = RouletteColorEnum.Black;
                    break;
                case "rouletteBetGreen":
                    playerChoose = RouletteColorEnum.Green;
                    break;
                default: return;
            }

            string answerMessage = await PlaceBetColorAsync(gameSession, playerChoose, user, dbContext, callbackQuery.From.FirstName, playerBetSum);

            Task sendingMessage = Client.TrySendTextMessageAsync(
                gameSession.ChatId,
                answerMessage,
                parseMode: ParseMode.Html);
            await Client.TryAnswerCallbackQueryAsync(callbackQuery.Id, allInAlertMessage);
            await sendingMessage;
        }
        public static async Task BetColorRequestAsync(Message message, string pattern)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();

            var gameSession = await GetGameSessionOrNullAsync(message.Chat.Id, dbContext);
            if (gameSession == null || gameSession.Resulting)
                return;

            UpdateLastActivity(gameSession);
            await dbContext.SaveChangesAsync();

            Match matchString = Regex.Match(message.Text, pattern, RegexOptions.IgnoreCase);

            long maxBetSum = ChapubelichClient.GetConfig().GetValue<long>("AppSettings:MaxBetSum");

            if (!long.TryParse(matchString.Groups[1].Value, out long playerBetSum) || playerBetSum > maxBetSum)
            {
                await Client.TrySendTextMessageAsync(
                    gameSession.ChatId,
                    $"Ты не можешь ставить больше <b>{maxBetSum}</b> 💵 за раз",
                    replyToMessageId: message.MessageId,
                    parseMode: ParseMode.Html);
                return;
            }
            if (playerBetSum == 0)
                return;

            char betColor = matchString.Groups[2].Value.ToLower().ElementAtOrDefault(0);

            User user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserId == message.From.Id);
            if (user == null)
                return;

            if (user.Balance == 0)
            {
                await Client.TrySendTextMessageAsync(
                        gameSession.ChatId,
                    $"<i><a href=\"tg://user?id={user.UserId}\">{message.From.FirstName}</a></i>, ты не можешь сделать ставку. У тебя нет денег 😞",
                    replyToMessageId: message.MessageId,
                    parseMode: ParseMode.Html);
                return;
            }

            string allInAlertMessage = string.Empty;
            if (playerBetSum >= user.Balance)
            {
                allInAlertMessage = "\n\nТы ставишь все свои средства!";
                playerBetSum = user.Balance;
            }

            // Определение ставки игрока
            RouletteColorEnum playerChoose;

            if (betColor == 'к' || betColor == 'r')
                playerChoose = RouletteColorEnum.Red;
            else if (betColor == 'ч' || betColor == 'b')
                playerChoose = RouletteColorEnum.Black;
            else if (betColor == 'з' || betColor == 'g')
                playerChoose = RouletteColorEnum.Green;
            else return;

            string answerMessage = await PlaceBetColorAsync(gameSession, playerChoose, user, dbContext, message.From.FirstName, playerBetSum);

            await Client.TrySendTextMessageAsync(
                gameSession.ChatId,
                answerMessage + allInAlertMessage,
                replyToMessageId: message.MessageId,
                parseMode: ParseMode.Html);

            if (!string.IsNullOrEmpty(matchString.Groups[3].Value))
                await EndGameSessionAsync(gameSession, dbContext, message.Chat.Type, message.MessageId);
        }
        public static async Task BetNumbersRequestAsync(CallbackQuery callbackQuery)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();

            var gameSession = await GetGameSessionOrNullAsync(callbackQuery.Message.Chat.Id, dbContext);
            if (gameSession == null || gameSession.Resulting)
                return;

            UpdateLastActivity(gameSession);
            await dbContext.SaveChangesAsync();

            User user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserId == callbackQuery.From.Id);

            if (user == null)
                return;

            if (user.Balance == 0)
            {
                await Client.TryAnswerCallbackQueryAsync(callbackQuery.Id,
                    "Ты не можешь сделать ставку. У тебя нет денег 😞");
                return;
            }

            long playerBetSum = user.DefaultBet;

            string allInAlertMessage = null;
            if (playerBetSum >= user.Balance)
            {
                allInAlertMessage = "Ты ставишь все свои средства!";
                playerBetSum = user.Balance;
            }

            int[] userBets = GetBetsByCallbackQuery(callbackQuery.Data);

            string answerMessage = await PlaceBetNumberAsync(gameSession, userBets, user, callbackQuery.From.FirstName, playerBetSum, dbContext);

            Task sendingMessage = Client.TrySendTextMessageAsync(
                gameSession.ChatId,
                answerMessage,
                parseMode: ParseMode.Html);
            await Client.TryAnswerCallbackQueryAsync(callbackQuery.Id, allInAlertMessage);
            await sendingMessage;
        }
        public static async Task BetNumbersRequestAsync(Message message, string pattern)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();

            var gameSession = await GetGameSessionOrNullAsync(message.Chat.Id, dbContext);
            if (gameSession == null || gameSession.Resulting)
                return; 

            UpdateLastActivity(gameSession);
            await dbContext.SaveChangesAsync();

            User user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserId == message.From.Id);

            if (user == null)
                return;

            if (user.Balance == 0)
            {
                await Client.TrySendTextMessageAsync(
                    gameSession.ChatId,
                    $"<i><a href=\"tg://user?id={user.UserId}\">{message.From.FirstName}</a></i>, ты не можешь сделать ставку. У тебя нет денег 😞",
                    replyToMessageId: message.MessageId,
                    parseMode: ParseMode.Html);
                return;
            }

            Match matchString = Regex.Match(message.Text, pattern, RegexOptions.IgnoreCase);

            long maxBetSum = ChapubelichClient.GetConfig().GetValue<long>("AppSettings:MaxBetSum");

            if (!long.TryParse(matchString.Groups[1].Value, out long playerBetSum) || playerBetSum > maxBetSum)
            {
                await Client.TrySendTextMessageAsync(
                    gameSession.ChatId,
                    $"Ты не можешь ставить больше <i>{maxBetSum}</i> 💵 за раз",
                    replyToMessageId: message.MessageId,
                    parseMode: ParseMode.Html);
                return;
            }
            
            int[] userBet;
            if (!string.IsNullOrEmpty(matchString.Groups[4].Value))
                userBet = matchString.Groups[4].Value[0] == 'ч' ? GetEvenBet() : GetOddBet();
            else
            {
                if (!int.TryParse(matchString.Groups[2].Value, out int firstNumber) ||
                    firstNumber > RouletteGameManager.TableSize)
                {
                    await Client.TrySendTextMessageAsync(
                        gameSession.ChatId,
                        "Некорректная ставка",
                        replyToMessageId: message.MessageId);
                    return;
                }

                if (!Int32.TryParse(matchString.Groups[3].Value, out int secondNumber))
                    userBet = GetBetsByNumbers(firstNumber);
                else
                {
                    int rangeSize = secondNumber - firstNumber + 1;

                    if (firstNumber >= secondNumber || secondNumber > RouletteGameManager.TableSize ||
                        secondNumber == 0)
                    {
                        await Client.TrySendTextMessageAsync(
                            gameSession.ChatId,
                            "Некорректная ставка",
                            replyToMessageId: message.MessageId);
                        return;
                    }

                    // Верификация ставки
                    string errorVerificationMessage = null;
                    if (!(rangeSize >= 2 && rangeSize <= 4) && (rangeSize != 6 && rangeSize != 12 && rangeSize != 18))
                    {
                        errorVerificationMessage = "Можно ставить только на последовательности из <b>2,3,4,6,12,18</b> чисел";
                    }
                    else if (rangeSize == 12 && firstNumber != 1 && firstNumber != 13 && firstNumber != 25)
                    {
                        errorVerificationMessage = "На дюжину можно ставить только <b>1-12</b>, <b>13-24</b>, <b>25-36</b>";
                    }
                    else if (rangeSize == 18 && firstNumber != 1 && firstNumber != 19)
                    {
                        errorVerificationMessage = "На выше/ниже можно ставить только <b>1-18</b>, <b>19-36</b>";
                    }

                    if (errorVerificationMessage != null)
                    {
                        await Client.TrySendTextMessageAsync(gameSession.ChatId,
                            errorVerificationMessage,
                            replyToMessageId: message.MessageId,
                            parseMode: ParseMode.Html);
                        return;
                    }

                    userBet = GetBetsByNumbers(firstNumber, secondNumber);
                }
            }

            string allInAlertMessage = string.Empty;
            if (playerBetSum >= user.Balance)
            {
                allInAlertMessage = "\n\nТы ставишь все свои средства!";
                playerBetSum = user.Balance;
            }

            string answerMessage = await PlaceBetNumberAsync(gameSession, userBet, user, message.From.FirstName, playerBetSum, dbContext);

            await Client.TrySendTextMessageAsync(
                gameSession.ChatId,
                answerMessage + allInAlertMessage,
                replyToMessageId: message.MessageId,
                parseMode: ParseMode.Html);

            if (!string.IsNullOrEmpty(matchString.Groups[5].Value))
                await EndGameSessionAsync(gameSession, dbContext, message.Chat.Type, message.MessageId);
        }
        public static async Task RollRequestAsync(CallbackQuery callbackQuery)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();

            var gameSession = await GetGameSessionOrNullAsync(callbackQuery.Message.Chat.Id, dbContext);
            if (gameSession == null || gameSession.Resulting)
                return;

            UpdateLastActivity(gameSession);
            await dbContext.SaveChangesAsync();

            if (gameSession.ColorBetTokens
                    .All(x => x.UserId != callbackQuery.From.Id)
                && gameSession.NumberBetTokens
                    .All(x => x.UserId != callbackQuery.From.Id))
            {
                await Client.TryAnswerCallbackQueryAsync(
                callbackQuery.Id,
                "Сделай ставку, чтобы крутить барабан");
                return;
            }

            Task answeringCallbackQuery = Client.TryAnswerCallbackQueryAsync(callbackQuery.Id, "Ты крутишь барабан! ✅");
            await EndGameSessionAsync(gameSession, dbContext, callbackQuery.Message.Chat.Type);
            await answeringCallbackQuery;
        }
        public static async Task RollRequestAsync(Message message)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            
            var gameSession = await GetGameSessionOrNullAsync(message.Chat.Id, dbContext);
            if (gameSession == null || gameSession.Resulting)
                return;

            UpdateLastActivity(gameSession);
            await dbContext.SaveChangesAsync();

            if (gameSession.ColorBetTokens
                    .All(x => x.UserId != message.From.Id)
                && gameSession.NumberBetTokens
                    .All(x => x.UserId != message.From.Id))
                await Client.TrySendTextMessageAsync(
                gameSession.ChatId,
                "Сделай ставку, чтобы крутить барабан",
                replyToMessageId: message.MessageId);
            else
                await EndGameSessionAsync(gameSession, dbContext, message.Chat.Type, message.MessageId);
        }
        public static async Task BetInfoRequestAsync(Message message)
        {
            User user;
            RouletteGameSession gameSession;
            await using (var dbContext = new ChapubelichdbContext())
            {
                gameSession = await GetGameSessionOrNullAsync(message.Chat.Id, dbContext);
                if (gameSession == null)
                    return;

                UpdateLastActivity(gameSession);
                await dbContext.SaveChangesAsync();

                user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserId == message.From.Id);
            }

            if (user == null)
                return;

            int userId = user.UserId;

            bool userHasTokens = gameSession.ColorBetTokens
                                     .Any(x => x.UserId == message.From.Id)
                                 || gameSession.NumberBetTokens
                                     .Any(x => x.UserId == message.From.Id);

            string transactionResult = string.Empty;
            if (!userHasTokens)
                transactionResult += $"<i><a href=\"tg://user?id={userId}\">{message.From.FirstName}</a></i>, у тебя нет активных ставок";
            else
                transactionResult += $"Ставка <i><a href=\"tg://user?id={userId}\">{message.From.FirstName}</a></i>:"
                                     + UserBetsToStringAsync(gameSession, userId);

            await Client.TrySendTextMessageAsync(
                gameSession.ChatId,
                transactionResult,
                replyToMessageId: message.MessageId,
                parseMode: ParseMode.Html);
        }

        public static async Task<RouletteGameSession> GetGameSessionOrNullAsync(long chatId, ChapubelichdbContext dbContext)
        {
            RouletteGameSession gameSession =
                await dbContext.RouletteGameSessions
                    .Include(gs => gs.ColorBetTokens)
                    .ThenInclude(bt => bt.User)
                    .Include(gs => gs.NumberBetTokens)
                    .ThenInclude(bt => bt.User)
                    .FirstOrDefaultAsync(x => x.ChatId == chatId);
            return gameSession;
        }

        // Private
        private static async Task<string> SummarizeAsync(RouletteGameSession gameSession)
        {
            StringBuilder result = new StringBuilder("Игра окончена.\nРезультат: ");
            result.Append($"<i>{gameSession.ResultNumber}</i> {gameSession.ResultNumber.ToRouletteColor().ToEmoji()}");

            List<RouletteColorBetToken> colorWinTokens = GetColorWinTokens(gameSession);
            List<RouletteColorBetToken> colorLooseTokens = GetColorLooseTokens(gameSession);
            List<RouletteNumbersBetToken> numberWinTokens = GetNumberWinTokens(gameSession);
            List<RouletteNumbersBetToken> numberLooseTokens = GetNumberLooseTokens(gameSession);

            List<RouletteBetToken> allWinTokens = new List<RouletteBetToken>(colorWinTokens.Count + numberWinTokens.Count);
            allWinTokens.AddRange(colorWinTokens);
            allWinTokens.AddRange(numberWinTokens);

            List<RouletteBetToken> allLooseTokens = new List<RouletteBetToken>(colorLooseTokens.Count + numberLooseTokens.Count);
            allLooseTokens.AddRange(colorLooseTokens);
            allLooseTokens.AddRange(numberLooseTokens);

            // Определение победителей
            if (allWinTokens.Any())
            {
                result.Append("\n🏆<b>Выиграли:</b>");
                foreach (var token in allWinTokens.GroupByUsers())
                {
                    long gainSum = token.GetGainSum();
                    User user = token.User;
                    if (user != null)
                    {
                        ChatMember chatMember = await Client.GetChatMemberAsync(gameSession.ChatId, user.UserId);
                        if (chatMember != null)
                        {
                            string userFirstName = chatMember.User.FirstName;
                            result.Append(
                                $"\n<b>·</b><i><a href=\"tg://user?id={user.UserId}\">{userFirstName}</a></i>: <b>+{gainSum.ToMoneyFormat()}</b>💵");
                        }
                        user.Balance += gainSum + token.BetSum;
                    }
                }
            }

            // Определение проигравших
            if (allLooseTokens.Any())
            {
                result.Append("\n😔<b>Проиграли:</b>");
                foreach (var token in allLooseTokens.GroupByUsers())
                {
                    User user = token.User;
                    if (user != null)
                    {
                        ChatMember chatMember = await Client.GetChatMemberAsync(gameSession.ChatId, user.UserId);
                        if (chatMember != null)
                        {
                            string userFirstName = chatMember.User.FirstName;
                            result.Append(
                                $"\n<b>·</b><i><a href=\"tg://user?id={user.UserId}\">{userFirstName}</a></i>: <b>-{token.BetSum.ToMoneyFormat()}</b>💵");
                        }
                    }
                }
            }

            return result.ToString();
        }
        private static async Task EndGameSessionAsync(RouletteGameSession gameSession, ChapubelichdbContext dbContext, ChatType chatType, int startMessageId = 0)
        {
            gameSession.Resulting = true;
            await dbContext.SaveChangesAsync();

            Message animationMessage = await Client.TrySendAnimationAsync(gameSession.ChatId,
                GetRandomAnimationLink(), disableNotification: true, caption: "Крутим барабан...");

            if (animationMessage != null)
            {
                gameSession.AnimationMessageId = animationMessage.MessageId;
                await dbContext.SaveChangesAsync();
            }

            int configAnimationDuration = ChapubelichClient.GetConfig().GetValue<int>("AppSettings:RouletteAnimationDuration");
            int animationDuration = (int)TimeSpan.FromSeconds(configAnimationDuration).TotalMilliseconds;
            int maxLimitAnimationDuration = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;
            Task task = Task.Delay(animationDuration >= maxLimitAnimationDuration ? maxLimitAnimationDuration : animationDuration);

            // Удаление сообщений и отправка результатов
            await UpdateLogAsync(gameSession, dbContext, chatType);
            string result = await SummarizeAsync(gameSession);

            await task;

            dbContext.Remove(gameSession);
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                Console.WriteLine("Повторное удаление сессии рулетки");
                return;
            }

            Task deletingAnimationMessage = Client.TryDeleteMessageAsync(gameSession.ChatId, gameSession.AnimationMessageId);

            Task deletingGameMessage = Client.TryDeleteMessageAsync(gameSession.ChatId, gameSession.GameMessageId);

            Task sendingResult = Client.TrySendTextMessageAsync(
                gameSession.ChatId,
                result,
                ParseMode.Html,
                replyMarkup: InlineKeyboards.RoulettePlayAgainMarkup,
                replyToMessageId: startMessageId);

            await deletingAnimationMessage;
            await deletingGameMessage;
            await sendingResult;
        }
        private static StringBuilder UserBetsToStringAsync(RouletteGameSession gameSession, int userId)
        {
            StringBuilder resultList = new StringBuilder();
            var userColorTokens = gameSession.ColorBetTokens.Where(x => x.UserId == userId).ToList();
            var userNumberTokens = gameSession.NumberBetTokens.Where(x => x.UserId == userId).ToList();

            var oneNumberUserTokens = userNumberTokens.Where(x => x.ChoosenNumbers?.Length == 1).ToList();
            var rangeUserTokens = userNumberTokens.Except(oneNumberUserTokens).ToList();

            foreach (var token in userColorTokens)
            {
                switch (token.ChoosenColor)
                {
                    case RouletteColorEnum.Red:
                        resultList.Append($"\n<b>{token.BetSum.ToMoneyFormat()}</b>: \U0001F534");
                        break;
                    case RouletteColorEnum.Black:
                        resultList.Append($"\n<b>{token.BetSum.ToMoneyFormat()}</b>: \U000026AB");
                        break;
                    case RouletteColorEnum.Green:
                        resultList.Append($"\n<b>{token.BetSum.ToMoneyFormat()}</b>: \U0001F7E2");
                        break;
                }
            }

            foreach (var token in rangeUserTokens)
            {
                if (token.ChoosenNumbers.IsSequenceBy(1))
                {
                    int firstnumber = token.ChoosenNumbers[0];
                    int secondNumber = token.ChoosenNumbers[^1];
                    resultList.Append($"\n<b>{token.BetSum.ToMoneyFormat()}</b>: (<i>{firstnumber} - {secondNumber}</i>)");
                }
                else
                {
                    if (token.ChoosenNumbers == null || token.ChoosenNumbers.Length <= 1)
                        return resultList;

                    resultList.Append($"\n<b>{token.BetSum.ToMoneyFormat()}</b>: (<i>{token.ChoosenNumbers[0]}");

                    for (int i = 1; i < token.ChoosenNumbers.Length; i++)
                    {
                        resultList.Append($", {token.ChoosenNumbers[i]}");
                    }
                    resultList.Append(")</i>");
                }
            }
            foreach (var token in oneNumberUserTokens)
            {
                resultList.Append($"\n<b>{token.BetSum.ToMoneyFormat()}</b>: (<i>{token.ChoosenNumbers[0]}</i> {token.ChoosenNumbers[0].ToRouletteColor().ToEmoji()})");
            }

            return resultList;
        }
        private static List<RouletteColorBetToken> GetColorWinTokens(RouletteGameSession gameSession)
        {
            return gameSession.ColorBetTokens
                .Where(x => x.ChoosenColor == gameSession.ResultNumber.ToRouletteColor()).ToList();
        }
        private static List<RouletteNumbersBetToken> GetNumberWinTokens(RouletteGameSession gameSession)
        {
            return gameSession.NumberBetTokens
                .Where(x => x.ChoosenNumbers
                    .Contains(gameSession.ResultNumber)).ToList();
        }
        private static List<RouletteColorBetToken> GetColorLooseTokens(RouletteGameSession gameSession)
        {
            return gameSession.ColorBetTokens.Where(x => x.ChoosenColor != gameSession.ResultNumber.ToRouletteColor()).ToList();
        }
        private static List<RouletteNumbersBetToken> GetNumberLooseTokens(RouletteGameSession gameSession)
        {
            return gameSession.NumberBetTokens
                .Where(x => !x.ChoosenNumbers
                    .Contains(gameSession.ResultNumber)).ToList();
        }

        private static async Task UpdateLogAsync(RouletteGameSession gameSession, ChapubelichdbContext dbContext, ChatType chatType)
        {
            if (chatType == ChatType.Private)
            {
                User user = await dbContext.Users
                    .FirstOrDefaultAsync(u => u.UserId == gameSession.ChatId);
                if (user != null)
                {
                    List<int> lastGameSessionsResults = user.LastGameSessions ??= new List<int>(1);
                    lastGameSessionsResults.Add(gameSession.ResultNumber);
                    while (user.LastGameSessions.Count > 10)
                        user.LastGameSessions.RemoveAt(0);
                }
            }
            else
            {
                Group group = await dbContext.Groups
                    .FirstOrDefaultAsync(g => g.GroupId == gameSession.ChatId);
                if (group != null)
                {
                    List<int> lastGameSessionsResults = group.LastGameSessions ??= new List<int>(1);
                    lastGameSessionsResults.Add(gameSession.ResultNumber);
                    while (group.LastGameSessions.Count > 10)
                        group.LastGameSessions.RemoveAt(0);
                }
            }
        }
        private static void UpdateLastActivity(RouletteGameSession gameSession)
        {
            gameSession.LastActivity = DateTime.UtcNow;
        }
        private static async Task CollectDeadSessionsAsync()
        {
            List<RouletteGameSession> deadSessions;
            await using (var dbContext = new ChapubelichdbContext())
            {
                int timeToSessionDispose = ChapubelichClient.GetConfig().GetValue<int>("AppSettings:StopGameDelay");

                deadSessions = (await dbContext.RouletteGameSessions
                    .Where(gs => gs.LastActivity < DateTime.UtcNow && !gs.Resulting)
                    .ToListAsync())
                    .Where(gs => gs.LastActivity.AddSeconds(timeToSessionDispose) < DateTime.UtcNow)
                    .ToList();
            }

            Parallel.ForEach(deadSessions, async gs =>
            {
                await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
                gs = await GetGameSessionOrNullAsync(gs.ChatId, dbContext);

                var returnedBets = string.Empty;

                var groupedColorBetList = gs.ColorBetTokens.GroupByUsers().ToList();
                var groupedNumberBetList = gs.NumberBetTokens.GroupByUsers().ToList();
                if (groupedColorBetList.Any() || groupedNumberBetList.Any())
                {
                    foreach (var bet in groupedColorBetList)
                    {
                        var user = bet.User;
                        if (user != null)
                            user.Balance += bet.BetSum;
                    }

                    foreach (var bet in groupedNumberBetList)
                    {
                        var user = bet.User;
                        if (user != null)
                            user.Balance += bet.BetSum;
                    }
                    returnedBets += "\nСтавки были возвращены 👍";
                }
                Task deletingMessage = null;
                if (gs.GameMessageId != 0)
                    deletingMessage = Client.TryDeleteMessageAsync(gs.ChatId, gs.GameMessageId);
                Task sendingMessage = Client.TrySendTextMessageAsync(
                    gs.ChatId,
                    "Игровая сессия <i>рулетки</i> отменена из-за отсутствия активности" + returnedBets,
                    ParseMode.Html,
                    replyMarkup: InlineKeyboards.RoulettePlayAgainMarkup);

                dbContext.RouletteGameSessions.Remove(gs);
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

        private static async Task<string> PlaceBetColorAsync(RouletteGameSession gameSession, RouletteColorEnum playerChoose, User user, ChapubelichdbContext dbContext, string firstName, long betSum)
        {
            var colorBetTokens = gameSession.ColorBetTokens;
            RouletteColorBetToken currentBetToken = colorBetTokens
                .FirstOrDefault(x =>
                    x.ChoosenColor == playerChoose && x.UserId == user.UserId);

            if (currentBetToken != null)
            {
                currentBetToken.BetSum += betSum;
            }
            else
            {
                currentBetToken = new RouletteColorBetToken(user, betSum, playerChoose);
                gameSession.ColorBetTokens.Add(currentBetToken);
            }

            user.Balance -= betSum;
            await dbContext.SaveChangesAsync();

            return $"<i><a href=\"tg://user?id={user.UserId}\">{firstName}</a></i>, ставка принята. Твоя суммарная ставка:"
                   + UserBetsToStringAsync(gameSession, user.UserId);
        }
        private static async Task<string> PlaceBetNumberAsync(RouletteGameSession gameSession, int[] userBets, User user, string firstName, long betSum, ChapubelichdbContext dbContext)
        {
            var numberBetTokens = gameSession.NumberBetTokens;
            RouletteNumbersBetToken currentBetToken = numberBetTokens
                .FirstOrDefault(x =>
                    x.ChoosenNumbers.SequenceEqual(userBets) && x.UserId == user.UserId);

            if (currentBetToken != null)
            {
                currentBetToken.BetSum += betSum;
            }
            else
            {
                currentBetToken = new RouletteNumbersBetToken(user, betSum, userBets);
                gameSession.NumberBetTokens.Add(currentBetToken);
            }

            user.Balance -= betSum;
            await dbContext.SaveChangesAsync();

            return $"<i><a href=\"tg://user?id={user.UserId}\">{firstName}</a></i>, ставка принята. Твоя суммарная ставка:"
                   + UserBetsToStringAsync(gameSession, user.UserId);
        }
        private static async Task<string> CancelBetAsync(RouletteGameSession gameSession, User user, string firstName, ChapubelichdbContext dbContext)
        {
            var userColorTokens = gameSession.ColorBetTokens.Where(x => x.UserId == user.UserId).ToList();
            var userNumberTokens = gameSession.NumberBetTokens.Where(x => x.UserId == user.UserId).ToList();

            if (userColorTokens.Any() || userNumberTokens.Any())
            {
                foreach (var token in userColorTokens)
                {
                    user.Balance += token.BetSum;
                }

                foreach (var token in userNumberTokens)
                {
                    user.Balance += token.BetSum;
                }

                gameSession.ColorBetTokens.RemoveAll(x => x.UserId == user.UserId);
                gameSession.NumberBetTokens.RemoveAll(x => x.UserId == user.UserId);
                await dbContext.SaveChangesAsync();
                return $"<i><a href=\"tg://user?id={user.UserId}\">{firstName}</a></i>, твоя ставка отменена \U0001F44D";
            }
            return $"<i><a href=\"tg://user?id={user.UserId}\">{firstName}</a></i>, у тебя нет активных ставок";
        }

        private static InputOnlineFile GetRandomAnimationLink()
        {
            string[] animationsLinks =
            {
                "https://media.giphy.com/media/uYDwaaoGJY26QDGzWr/giphy-downsized.gif",
                "https://media.giphy.com/media/erhiQf3RxVD6Sc8wXL/giphy-downsized.gif",
                "https://media.giphy.com/media/E8ucUBt3iTSR2Os1Dz/giphy.gif",
                "https://media.giphy.com/media/zDPm9BrrKWZqxGiyxG/giphy.gif",
                "https://media.giphy.com/media/XQyukWswMTViqZKft8/giphy.gif",
                "https://media.giphy.com/media/ats5YZpBxdvEpxLvcd/giphy.gif",
                "https://media.giphy.com/media/Bcwk17rD2eROmcZ1Di/giphy.gif",
                "https://media.giphy.com/media/MsUVF6GCUwEAZhNeTp/giphy.gif"
            };

            Random random = new Random();
            return new InputOnlineFile(animationsLinks[random.Next(0, animationsLinks.Length)]);
        }
        private static int GetRandomResultNumber()
        {
            Random rand = new Random();
            return rand.Next(0, TableSize - 1);
        }
        private static int[] GetBetsByNumbers(int firstNumber)
        {
            return new[]
            {
                firstNumber
            };
        }
        private static int[] GetBetsByNumbers(int firstNumber, int secondNumber)
        {
            int betSize = secondNumber - firstNumber + 1;
            int[] bets = new int[betSize];
            for (int i = 0; i < betSize; i++)
            {
                bets[i] = firstNumber + i;
            }
            return bets;
        }
        private static int[] GetBetsByCallbackQuery(string queryData)
        {
            int[] userBet;
            switch (queryData)
            {
                case "rouletteBetEven":
                    return GetEvenBet();
                case "rouletteBetOdd":
                    return GetOddBet();

                case "rouletteBetFirstHalf":
                    return GetBetsByNumbers(1, (TableSize - 1) / 2);
                case "rouletteBetSecondHalf":
                    return GetBetsByNumbers(((TableSize - 1) / 2) + 1, TableSize - 1);

                case "rouletteBetFirstTwelve":
                    return GetBetsByNumbers(1, (TableSize - 1) / 3);
                case "rouletteBetSecondTwelve":
                    int dividedByThree = (TableSize - 1) / 3;
                    return GetBetsByNumbers(dividedByThree + 1, dividedByThree * 2);
                case "rouletteBetThirdTwelve":
                    dividedByThree = (TableSize - 1) / 3;
                    return GetBetsByNumbers((dividedByThree * 2) + 1, dividedByThree * 3);

                case "rouletteBetFirstRow":
                    userBet = new int[(TableSize - 1) / 3];
                    for (int i = 0, j = 1; i < userBet.Length; i++)
                    {
                        userBet[i] = j;
                        j += 3;
                    }
                    return userBet;
                case "rouletteBetSecondRow":
                    userBet = new int[(TableSize - 1) / 3];
                    for (int i = 0, j = 2; i < userBet.Length; i++)
                    {
                        userBet[i] = j;
                        j += 3;
                    }
                    return userBet;
                case "rouletteBetThirdRow":
                    userBet = new int[(TableSize - 1) / 3];
                    for (int i = 0, j = 3; i < userBet.Length; i++)
                    {
                        userBet[i] = j;
                        j += 3;
                    }
                    return userBet;
            }

            return null;
        }

        private static int[] GetEvenBet()
        {
            int[] userBet = new int[(TableSize - 1) / 2];
            for (int i = 0, j = 1; i < userBet.Length; j++)
            {
                if (j % 2 == 0)
                {
                    userBet[i] = j;
                    i++;
                }
            }

            return userBet;
        }
        private static int[] GetOddBet()
        {
            int[] userBet = new int[(TableSize - 1) / 2];
            for (int i = 0, j = 1; i < userBet.Length; j++)
            {
                if (j % 2 != 0)
                {
                    userBet[i] = j;
                    i++;
                }
            }

            return userBet;
        }
    }
}
