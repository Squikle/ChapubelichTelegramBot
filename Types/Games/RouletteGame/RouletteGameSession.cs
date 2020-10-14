using ChapubelichBot.Types.Statics;
using ChapubelichBot.Database;
using ChapubelichBot.Types.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using User = ChapubelichBot.Database.Models.User;
using ChapubelichBot.Types.Enums;
using ChapubelichBot.Types.Abstractions;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using ChapubelichBot.Init;

namespace ChapubelichBot.Types.Games.RouletteGame
{
    class RouletteGameSession : IDisposable
    {
        public long ChatId { get; set; }
        private Message GameMessage { get; set; }
        private List<RouletteBetToken> BetTokens { get; set; }
        private bool Resulting { get; set; }
        private int ResultNumber { get; set; }
        private Timer Timer;

        public RouletteGameSession(Message message, ITelegramBotClient client)
        {
            BetTokens = new List<RouletteBetToken>();
            ChatId = message.Chat.Id;
            Timer = new Timer(x => DisposeAfterTime(client), null, AppSettings.StopGameDelay, AppSettings.StopGameDelay);
        }

        public async Task StartAsync(Message message, ITelegramBotClient client)
        {
            Resulting = false;

            ResultNumber = RouletteTableStatic.GetRandomResultNumber();
            int replyId = message.From.Id == client.BotId ? 0 : message.MessageId;

            GameMessage = await client.TrySendPhotoAsync(message.Chat.Id,
                "https://i.imgur.com/SN8DRoa.png",
                caption: "Игра запущена. Ждем ваши ставки...\n" +
                "Вы можете поставить ставку по умолчанию на предложенные ниже варианты:",
                replyToMessageId: replyId,
                replyMarkup: InlineKeyboardsStatic.rouletteBetsMarkup);
        }
        public async Task ResultAsync(ITelegramBotClient client, Message startMessage = null)
        {
            if (Resulting)
                return;

            Resulting = true;
            Message animationMessage = await client.TrySendAnimationAsync(ChatId, GetRandomAnimationLink(), disableNotification: true, caption: "Крутим барабан...");

            Task task = Task.Delay(AppSettings.RouletteAnimationDuration);
            // Удаление сообщений и отправка результатов
            string result = (await SummarizeAsync()).ToString();
            await task;

            if (animationMessage != null)
                await client.TryDeleteMessageAsync(animationMessage.Chat.Id, animationMessage.MessageId);

            if (GameMessage != null)
                await client.TryDeleteMessageAsync(GameMessage.Chat.Id, GameMessage.MessageId);

            int replyId = startMessage == null ? 0 : startMessage.MessageId;
            await client.TrySendTextMessageAsync(
                ChatId,
                result,
                Telegram.Bot.Types.Enums.ParseMode.Html,
                replyMarkup: InlineKeyboardsStatic.roulettePlayAgainMarkup,
                replyToMessageId: replyId);

            Dispose();
        }
        private async Task<StringBuilder> SummarizeAsync()
        {
            RouletteColorEnum resultColor = ResultNumber.ToRouletteColor();

            StringBuilder result = new StringBuilder("Игра окончена.\nРезультат: ");
            result.Append($"{ResultNumber} {resultColor.ToEmoji()}");

            // Токены с цветом
            var winTokensColor = BetTokens.OfType<RouletteColorBetToken>().Where(x => x.ChoosenColor == resultColor).ToList();
            var looseTokensColor = BetTokens.OfType<RouletteColorBetToken>().Where(x => x.ChoosenColor != resultColor).ToList();

            // Токены с числами
            var winTokensNumbers = BetTokens.OfType<RouletteNumbersBetToken>().Where(x => x.ChoosenNumbers != null && x.ChoosenNumbers.Contains(ResultNumber)).ToList();
            var looseTokensNumbers = BetTokens.OfType<RouletteNumbersBetToken>().Where(x => x.ChoosenNumbers != null && !x.ChoosenNumbers.Contains(ResultNumber)).ToList();

            var winTokens = new List<RouletteBetToken>(winTokensColor.Count + winTokensColor.Count);
            winTokens.AddRange(winTokensColor);
            winTokens.AddRange(winTokensNumbers);

            var looseTokens = new List<RouletteBetToken>(looseTokensColor.Count + looseTokensNumbers.Count);
            looseTokens.AddRange(looseTokensColor);
            looseTokens.AddRange(looseTokensNumbers);

            using (var db = new ChapubelichdbContext())
            {
                // Определение победителей
                if (winTokens.Any())
                {
                    result.Append("\n🏆<b>Выиграли:</b>");
                    foreach (var token in winTokens.GroupByUsers())
                    {
                        int gainSum = token.GetGainSum();
                        User user = db.Users.FirstOrDefault(x => x.UserId == token.UserId);
                        result.Append($"\n<b>·</b><a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>: <b>+{gainSum.ToMoneyFormat()}</b>💵");
                        user.Balance += gainSum + token.BetSum;
                    }
                }
                // Определение проигравших
                if (looseTokens.Any())
                {
                    result.Append("\n\U0001F614<b>Проиграли:</b>");
                    foreach (var player in looseTokens.GroupByUsers())
                    {
                        User user = db.Users.FirstOrDefault(x => x.UserId == player.UserId);
                        result.Append($"\n<b>·</b><a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>: <b>-{player.BetSum.ToMoneyFormat()}</b>💵");
                    }
                }

                await db.SaveChangesAsync();
            }

            return result;
        }
        public StringBuilder UserBetsToStringAsync(User user)
        {
            StringBuilder resultList = new StringBuilder();
            var userTokens = BetTokens.Where(x => x.UserId == user.UserId);

            var colorUserTokens = userTokens.OfType<RouletteColorBetToken>();

            var numberUserTokens = userTokens.OfType<RouletteNumbersBetToken>();
            var oneNumberUserTokens = numberUserTokens.Where(x => x.ChoosenNumbers?.Length == 1);
            var rangeUserTokens = numberUserTokens.Except(oneNumberUserTokens);

            foreach (var token in colorUserTokens)
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
                    int secondNumber = token.ChoosenNumbers[token.ChoosenNumbers.Length - 1];
                    resultList.Append($"\n<b>{token.BetSum.ToMoneyFormat()}</b>: ({firstnumber} - {secondNumber})");
                }
                else
                {
                    if (token.ChoosenNumbers == null || token.ChoosenNumbers.Length <= 1)
                        return resultList;

                    resultList.Append($"\n<b>{token.BetSum.ToMoneyFormat()}</b>: ({token.ChoosenNumbers[0]}");

                    for (int i = 1; i < token.ChoosenNumbers.Length; i++)
                    {
                        resultList.Append($", {token.ChoosenNumbers[i]}");
                    }
                    resultList.Append(")");
                }
            }
            foreach (var token in oneNumberUserTokens)
            {
                resultList.Append($"\n<b>{token.BetSum}</b> ({token.ChoosenNumbers[0]} {token.ChoosenNumbers[0].ToRouletteColor().ToEmoji()})");
            }

            return resultList;
        }

        public async Task BetCancel(CallbackQuery callbackQuery, ITelegramBotClient client)
        {
            DelayTimer();

            if (Resulting)
                return;

            using (var db = new ChapubelichdbContext())
            {
                User user = db.Users.First(x => x.UserId == callbackQuery.From.Id);
                if (user == null)
                    return;
                var userTokens = BetTokens.Where(x => x.UserId == callbackQuery.From.Id);

                if (userTokens.Any())
                {
                    foreach (var token in userTokens)
                    {
                        user.Balance += token.BetSum;
                    }
                    BetTokens = BetTokens.Except(userTokens).ToList();

                    await db.SaveChangesAsync();

                    await client.TrySendTextMessageAsync(
                            ChatId,
                            $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, Ваша ставка отменена \U0001F44D",
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

                    await client.TryAnswerCallbackQueryAsync(callbackQuery.Id, "✅");
                }
                else
                    await client.TryAnswerCallbackQueryAsync(callbackQuery.Id, "у Вас нет активных ставок");
            }
        }
        public async Task BetCancel(Message message, ITelegramBotClient client)
        {
            DelayTimer();

            if (Resulting)
                return;

            using (var db = new ChapubelichdbContext())
            {
                User user = db.Users.First(x => x.UserId == message.From.Id);
                if (user == null)
                    return;
                var userTokens = BetTokens.Where(x => x.UserId == message.From.Id);
                if (userTokens.Any())
                {
                    foreach (var token in userTokens)
                    {
                        user.Balance += token.BetSum;
                    }
                    BetTokens = BetTokens.Except(userTokens).ToList();

                    await db.SaveChangesAsync();

                    await client.TrySendTextMessageAsync(
                            ChatId,
                            $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, Ваша ставка отменена \U0001F44D",
                            replyToMessageId: message.MessageId,
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                }

                else await client.TrySendTextMessageAsync(
                            ChatId,
                            $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, у Вас нет активных ставок",
                            replyToMessageId: message.MessageId,
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }
        }
        public async Task BetColor(CallbackQuery callbackQuery, ITelegramBotClient client)
        {
            DelayTimer();

            if (Resulting)
            {
                await client.TryAnswerCallbackQueryAsync(callbackQuery.Id,
                        "Барабан уже крутится, слишком поздно для ставок");
                return;
            }

            using (var db = new ChapubelichdbContext())
            {
                User user = db.Users.FirstOrDefault(x => x.UserId == callbackQuery.From.Id);
                if (user == null)
                    return;

                if (user.DefaultBet > user.Balance)
                {
                    await client.TryAnswerCallbackQueryAsync(callbackQuery.Id,
                        "У вас недостаточно средств на счету");
                    return;
                }

                RouletteColorEnum playerChoose = RouletteColorEnum.Red;


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

                var colorBetTokens = BetTokens.OfType<RouletteColorBetToken>();
                RouletteColorBetToken currentBetToken = colorBetTokens.FirstOrDefault(x => x.ChoosenColor == playerChoose && x.UserId == user.UserId);

                if (currentBetToken != null)
                    currentBetToken.BetSum += user.DefaultBet;
                else
                {
                    currentBetToken = new RouletteColorBetToken(user, user.DefaultBet, playerChoose);
                    BetTokens.Add(currentBetToken);
                }

                user.Balance -= user.DefaultBet;
                await db.SaveChangesAsync();

                string transactionResult = $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, ставка принята. Ваша суммарная ставка:"
                    + UserBetsToStringAsync(user);

                await client.TrySendTextMessageAsync(
                    callbackQuery.Message.Chat.Id,
                    transactionResult,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                await client.TryAnswerCallbackQueryAsync(callbackQuery.Id);
            }
        }
        public async Task BetColor(Message message, string pattern, ITelegramBotClient client)
        {
            DelayTimer();

            if (Resulting)
            {
                await client.TrySendTextMessageAsync(message.Chat.Id,
                        "Барабан уже крутится, слишком поздно для ставок",
                        replyToMessageId: message.MessageId);
                return;
            }

            Match matchString = Regex.Match(message.Text, pattern, RegexOptions.IgnoreCase);

            if (!Int32.TryParse(matchString.Groups[1].Value, out int playerBet) || playerBet == 0)
                return;
            char betColor = matchString.Groups[2].Value.ToLower().ElementAtOrDefault(0);

            // Определение ставки игрока
            RouletteColorEnum playerChoose = RouletteColorEnum.Red;

            if (betColor == 'к' || betColor == 'r')
                playerChoose = RouletteColorEnum.Red;
            else if (betColor == 'ч' || betColor == 'b')
                playerChoose = RouletteColorEnum.Black;
            else if (betColor == 'з' || betColor == 'g')
                playerChoose = RouletteColorEnum.Green;
            else return;

            using (var db = new ChapubelichdbContext())
            {
                User user = db.Users.FirstOrDefault(x => x.UserId == message.From.Id);
                if (user == null)
                    return;

                if (playerBet > user.Balance)
                {
                    await client.TrySendTextMessageAsync(message.Chat.Id,
                        "У вас недостаточно средств на счету\U0001F614",
                        replyToMessageId: message.MessageId);
                    return;
                }

                var colorBetTokens = BetTokens.OfType<RouletteColorBetToken>();
                RouletteColorBetToken currentBetToken = colorBetTokens.FirstOrDefault(x => x.ChoosenColor == playerChoose && x.UserId == user.UserId);

                if (null != currentBetToken)
                    currentBetToken.BetSum += playerBet;
                else
                {
                    currentBetToken = new RouletteColorBetToken(user, playerBet, playerChoose);
                    BetTokens.Add(currentBetToken);
                }

                user.Balance -= playerBet;
                await db.SaveChangesAsync();

                string transactionResult = $"Ставка принята. Суммарная ставка <a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>:"
                    + UserBetsToStringAsync(user);

                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    transactionResult,
                    replyToMessageId: message.MessageId,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

                if (!string.IsNullOrEmpty(matchString.Groups[9].Value))
                    await ResultAsync(client, message);
            }
        }
        public async Task BetNumbers(CallbackQuery callbackQuery, ITelegramBotClient client)
        {
            DelayTimer();

            if (Resulting)
            {
                await client.TryAnswerCallbackQueryAsync(callbackQuery.Id,
                        "Барабан уже крутится, слишком поздно для ставок");
                return;
            }

            using (var db = new ChapubelichdbContext())
            {
                User user = db.Users.FirstOrDefault(x => x.UserId == callbackQuery.From.Id);
                if (user == null)
                    return;

                if (user.DefaultBet > user.Balance)
                {
                    await client.TryAnswerCallbackQueryAsync(callbackQuery.Id,
                        "У вас недостаточно средств на счету");
                    return;
                }

                int[] userBets = RouletteTableStatic.GetBetsByCallbackQuery(callbackQuery.Data);

                var numberBetTokens = BetTokens.OfType<RouletteNumbersBetToken>();
                RouletteNumbersBetToken currentBetToken = numberBetTokens.FirstOrDefault(x => x.ChoosenNumbers.SequenceEqual(userBets) && x.UserId == user.UserId);

                if (currentBetToken != null)
                    currentBetToken.BetSum += user.DefaultBet;
                else
                {
                    currentBetToken = new RouletteNumbersBetToken(user, user.DefaultBet, userBets);
                    BetTokens.Add(currentBetToken);
                }

                user.Balance -= user.DefaultBet;
                await db.SaveChangesAsync();

                string transactionResult = $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, ставка принята. Ваша суммарная ставка:"
                    + UserBetsToStringAsync(user);

                await client.TrySendTextMessageAsync(
                    callbackQuery.Message.Chat.Id,
                    transactionResult,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                await client.TryAnswerCallbackQueryAsync(callbackQuery.Id);
            }
        }
        public async Task BetNumbers(Message message, string pattern, ITelegramBotClient client)
        {
            DelayTimer();

            if (Resulting)
            {
                await client.TrySendTextMessageAsync(message.Chat.Id,
                        "Барабан уже крутится, слишком поздно для ставок",
                        replyToMessageId: message.MessageId);
                return;
            }

            Match matchString = Regex.Match(message.Text, pattern, RegexOptions.IgnoreCase);

            int[] userBets;

            if (!Int32.TryParse(matchString.Groups[1].Value, out int playerBet))
                return;
            if (!Int32.TryParse(matchString.Groups[2].Value, out int firstNumber) || firstNumber > RouletteTableStatic.tableSize)
                return;
            if (!Int32.TryParse(matchString.Groups[4].Value, out int secondNumber))
                userBets = RouletteTableStatic.GetBetsByNumbers(firstNumber);
            else
            {
                int rangeSize = secondNumber - firstNumber + 1;

                if (firstNumber >= secondNumber || secondNumber > RouletteTableStatic.tableSize || secondNumber == 0)
                    return;

                // Валидация ставки
                if (!(rangeSize >= 2 && rangeSize <= 4) && (rangeSize != 4 && rangeSize != 6 && rangeSize != 12 && rangeSize != 18))
                {
                    await client.TrySendTextMessageAsync(message.Chat.Id,
                       "Можно ставить только на последовательности из 2,3,4,6,12,18 чисел",
                       replyToMessageId: message.MessageId);
                    return;
                }
                if (rangeSize == 12 && firstNumber != 1 && firstNumber != 13 && firstNumber != 25)
                {
                    await client.TrySendTextMessageAsync(message.Chat.Id,
                       "На дюжину можно ставить только 1-12, 13-24, 25-36",
                       replyToMessageId: message.MessageId);
                    return;
                }
                if (rangeSize == 18 && firstNumber != 1 && firstNumber != 19)
                {
                    await client.TrySendTextMessageAsync(message.Chat.Id,
                       "На выше/ниже можно ставить только 1-18, 19-36",
                       replyToMessageId: message.MessageId);
                    return;
                }

                userBets = RouletteTableStatic.GetBetsByNumbers(firstNumber, secondNumber);
            }

            using (var db = new ChapubelichdbContext())
            {
                User user = db.Users.FirstOrDefault(x => x.UserId == message.From.Id);
                if (user == null)
                    return;

                if (playerBet > user.Balance)
                {
                    await client.TrySendTextMessageAsync(message.Chat.Id,
                        "У вас недостаточно средств на счету\U0001F614",
                        replyToMessageId: message.MessageId);
                    return;
                }

                var numberBetTokens = BetTokens.OfType<RouletteNumbersBetToken>();
                RouletteNumbersBetToken currentBetToken = numberBetTokens.FirstOrDefault(x => x.ChoosenNumbers.SequenceEqual(userBets) && x.UserId == user.UserId);

                if (currentBetToken != null)
                    currentBetToken.BetSum += playerBet;
                else
                {
                    currentBetToken = new RouletteNumbersBetToken(user, playerBet, userBets);
                    BetTokens.Add(currentBetToken);
                }

                user.Balance -= playerBet;
                await db.SaveChangesAsync();

                string transactionResult = $"Ставка принята. Суммарная ставка <a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>:"
                    + UserBetsToStringAsync(user);

                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    transactionResult,
                    replyToMessageId: message.MessageId,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

                if (!string.IsNullOrEmpty(Regex.Match(message.Text, pattern, RegexOptions.IgnoreCase).Groups[5].Value))
                    await ResultAsync(client, message);
            }
        }
        public async Task Roll(CallbackQuery callbackQuery, ITelegramBotClient client)
        {
            DelayTimer();

            if (!BetTokens.Any(x => x.UserId == callbackQuery.From.Id))
            {
                await client.TryAnswerCallbackQueryAsync(
                callbackQuery.Id,
                "Чтобы крутить барабан сделайте ставку");
                return;
            }

            await ResultAsync(client);
            await client.TryAnswerCallbackQueryAsync(callbackQuery.Id, "✅");
        }
        public async Task Roll(Message message, ITelegramBotClient client)
        {
            DelayTimer();

            if (!BetTokens.Any(x => x.UserId == message.From.Id))
                await client.TrySendTextMessageAsync(
                message.Chat.Id,
                "Чтобы крутить барабан сделайте ставку",
                replyToMessageId: message.MessageId,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            else
                await ResultAsync(client, message);
        }
        public async Task CheckBet(Message message, ITelegramBotClient client)
        {
            DelayTimer();

            using (var db = new ChapubelichdbContext())
            {
                User user = db.Users.First(x => x.UserId == message.From.Id);
                var userTokens = BetTokens.Where(x => x.UserId == message.From.Id);

                string transactionResult = string.Empty;
                if (!userTokens.Any())
                    transactionResult += $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, у Вас нет активных ставок";
                else
                    transactionResult += $"Ставка <a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>:"
                        + UserBetsToStringAsync(user);

                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    transactionResult,
                    replyToMessageId: message.MessageId,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }
        }

        public void DelayTimer()
        {
            Timer.Change(AppSettings.StopGameDelay, AppSettings.StopGameDelay);
        }
        public async static Task<Message> InitializeNew(Message message, ITelegramBotClient client)
        {
            RouletteGameSession gameSession = RouletteTableStatic.GetGameSessionOrNull(message.Chat.Id);

            if (gameSession != null)
                return gameSession.GameMessage;

            gameSession = new RouletteGameSession(message, client);
            RouletteTableStatic.GameSessions.Add(gameSession);
            await gameSession.StartAsync(message, client);
            return null;
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

        public async void DisposeAfterTime(ITelegramBotClient client)
        {
            if (Resulting)
                return;

            Resulting = true;
            var returnedBets = string.Empty;
            using (var db = new ChapubelichdbContext())
            {
                var groupedBetList = BetTokens.GroupByUsers();
                if (groupedBetList.Any())
                {
                    foreach (var bet in groupedBetList)
                    {
                        db.Users.FirstOrDefault(x => x.UserId == bet.UserId).Balance += bet.BetSum;
                    }
                    returnedBets += "\nСтавки были возвращены👍";
                }
                await db.SaveChangesAsync();
            }

            if (GameMessage != null)
            {
                await client.TryDeleteMessageAsync(GameMessage.Chat.Id, GameMessage.MessageId);
            }
            await client.TrySendTextMessageAsync(
            ChatId,
            "Игровая сессия отменена из-за отсутствия активности" + returnedBets,
            Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: InlineKeyboardsStatic.roulettePlayAgainMarkup);

            Dispose();
        }
        public void Dispose()
        {
            RouletteTableStatic.GameSessions.Remove(this);
            Timer.Dispose();
        }
    }
}
