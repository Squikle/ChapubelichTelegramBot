using ChapubelichBot.Types.Statics;
using ChapubelichBot.Database;
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
using ChapubelichBot.Types.Extensions;
using Microsoft.Extensions.Configuration;

namespace ChapubelichBot.Types.Games.RouletteGame
{
    class RouletteGameSession : IDisposable
    {
        public long ChatId { get; set; }
        public Message GameMessage { get; set; }
        private List<RouletteBetToken> BetTokens { get; set; }
        private bool Resulting { get; set; }
        private int ResultNumber { get; set; }
        private readonly Timer _timer;
        private readonly int _stopGameDelay = Bot.GetConfig().GetValue<int>("AppSettings:StopGameDelay") * 1000;

        public RouletteGameSession(long chatId, ITelegramBotClient client)
        {
            BetTokens = new List<RouletteBetToken>();
            ChatId = chatId;

            _timer = new Timer(x => DisposeAfterTime(client), null, _stopGameDelay, _stopGameDelay);
        }

        private async Task ResultAsync(ITelegramBotClient client, Message startMessage = null)
        {
            if (Resulting)
                return;

            Resulting = true;
            Message animationMessage = await client.TrySendAnimationAsync(ChatId, GetRandomAnimationLink(), disableNotification: true, caption: "Крутим барабан...");

            int configAnimationDuration = Bot.GetConfig().GetValue<int>("AppSettings:RouletteAnimationDuration") * 1000;
            Task task = Task.Delay(configAnimationDuration >= 10*1000 ? 10000 : configAnimationDuration);

            // Удаление сообщений и отправка результатов
            var db = new ChapubelichdbContext();
            string result = Summarize(db).ToString();
            await task;

            if (animationMessage != null)
                await client.TryDeleteMessageAsync(animationMessage.Chat.Id, animationMessage.MessageId);

            if (GameMessage != null)
                await client.TryDeleteMessageAsync(GameMessage.Chat.Id, GameMessage.MessageId);

            int replyId = startMessage?.MessageId ?? 0;
            await client.TrySendTextMessageAsync(
                ChatId,
                result,
                Telegram.Bot.Types.Enums.ParseMode.Html,
                replyMarkup: InlineKeyboards.RoulettePlayAgainMarkup,
                replyToMessageId: replyId);

            await db.SaveChangesAsync();
            await db.DisposeAsync();
            Dispose();
        }
        private StringBuilder Summarize(ChapubelichdbContext db)
        {
            StringBuilder result = new StringBuilder("Игра окончена.\nРезультат: ");
            result.Append($"{ResultNumber} {ResultNumber.ToRouletteColor().ToEmoji()}");

            List<RouletteBetToken> winTokens = GetWinTokensGroupedByUsers().ToList();
            List<RouletteBetToken> looseTokens = GetLooseTokensGroupedByUsers().ToList();

            //TODO закрывать бд после прохода по выигрышам, т.к. в проигрышных нужен только массив юзеров
            // Определение победителей
            if (winTokens.Any())
            {
                result.Append("\n🏆<b>Выиграли:</b>");
                foreach (var token in winTokens.GroupByUsers())
                {
                    int gainSum = token.GetGainSum();
                    User user = db.Users.FirstOrDefault(x => x.UserId == token.UserId);
                    if (user != null)
                    {
                        result.Append(
                            $"\n<b>·</b><a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>: <b>+{gainSum.ToMoneyFormat()}</b>💵");
                        user.Balance += gainSum + token.BetSum;
                    }
                }
            }
            // Определение проигравших
            if (looseTokens.Any())
            {
                result.Append("\n\U0001F614<b>Проиграли:</b>");
                foreach (var player in looseTokens.GroupByUsers())
                {
                    User user = db.Users.FirstOrDefault(x => x.UserId == player.UserId);
                    if (user != null)
                        result.Append(
                            $"\n<b>·</b><a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>: <b>-{player.BetSum.ToMoneyFormat()}</b>💵");
                }
            }

            return result;
        }
        private StringBuilder UserBetsToStringAsync(User user)
        {
            StringBuilder resultList = new StringBuilder();
            var userTokens = BetTokens.Where(x => x.UserId == user.UserId).ToList();

            var numberUserTokens = userTokens.OfType<RouletteNumbersBetToken>().ToList();
            var oneNumberUserTokens = numberUserTokens.Where(x => x.ChoosenNumbers?.Length == 1).ToList();
            var rangeUserTokens = numberUserTokens.Except(oneNumberUserTokens).ToList();

            foreach (var token in userTokens.OfType<RouletteColorBetToken>())
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
        private IEnumerable<RouletteBetToken> GetWinTokensGroupedByUsers()
        {
            var winTokensColor = BetTokens.OfType<RouletteColorBetToken>().Where(x => x.ChoosenColor == ResultNumber.ToRouletteColor()).ToList();
            var winTokensNumbers = BetTokens.OfType<RouletteNumbersBetToken>().Where(x => x.ChoosenNumbers.Contains(ResultNumber)).ToList();

            var winTokens = new List<RouletteBetToken>(winTokensColor.Count + winTokensColor.Count);
            winTokens.AddRange(winTokensColor);
            winTokens.AddRange(winTokensNumbers);

            return winTokens.GroupByUsers();
        }
        private IEnumerable<RouletteBetToken> GetLooseTokensGroupedByUsers()
        {
            var looseTokensColor = BetTokens.OfType<RouletteColorBetToken>().Where(x => x.ChoosenColor != ResultNumber.ToRouletteColor()).ToList();
            var looseTokensNumbers = BetTokens.OfType<RouletteNumbersBetToken>().Where(x => !x.ChoosenNumbers.Contains(ResultNumber)).ToList();

            var looseTokens = new List<RouletteBetToken>(looseTokensColor.Count + looseTokensNumbers.Count);
            looseTokens.AddRange(looseTokensColor);
            looseTokens.AddRange(looseTokensNumbers);

            return looseTokens.GroupByUsers();
        }
        private async void DisposeAfterTime(ITelegramBotClient client)
        {
            if (Resulting)
                return;

            Resulting = true;
            var returnedBets = string.Empty;
            await using (var db = new ChapubelichdbContext())
            {
                var groupedBetList = BetTokens.GroupByUsers().ToList();
                if (groupedBetList.Any())
                {
                    foreach (var bet in groupedBetList)
                    {
                        var user = db.Users.FirstOrDefault(x => x.UserId == bet.UserId);
                        if (user != null)
                            user.Balance += bet.BetSum;
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
                replyMarkup: InlineKeyboards.RoulettePlayAgainMarkup);

            Dispose();
        }
        private void DelayTimer()
        {
            _timer.Change(_stopGameDelay, _stopGameDelay);
        }
        public void Dispose()
        {
            Statics.RouletteGame.GameSessions.Remove(this);
            _timer.Dispose();
        }

        public async Task InitSessionAsync(Message message, ITelegramBotClient client)
        {
            Resulting = false;

            ResultNumber = Statics.RouletteGame.GetRandomResultNumber();

            int replyId = message.From.Id == client.BotId ? 0 : message.MessageId;

            GameMessage = await client.TrySendPhotoAsync(message.Chat.Id,
                "https://i.imgur.com/SN8DRoa.png",
                caption: "Игра запущена. Ждем ваши ставки...\n" +
                         "Ты можешь поставить ставку по умолчанию на предложенные ниже варианты:",
                replyToMessageId: replyId,
                replyMarkup: InlineKeyboards.RouletteBetsMarkup);
        }
        public async Task BetCancelRequest(CallbackQuery callbackQuery, ITelegramBotClient client)
        {
            DelayTimer();

            if (Resulting)
                return;

            await using var db = new ChapubelichdbContext();
            User user = db.Users.First(x => x.UserId == callbackQuery.From.Id);
            if (user == null)
                return;

            string answerMessage = CancelBet(user);
            await db.SaveChangesAsync();

            await client.TrySendTextMessageAsync(
                ChatId,
                answerMessage,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

            await client.TryAnswerCallbackQueryAsync(callbackQuery.Id);
        }
        public async Task BetCancelRequest(Message message, ITelegramBotClient client)
        {
            DelayTimer();

            if (Resulting)
                return;

            await using var db = new ChapubelichdbContext();
            User user = db.Users.First(x => x.UserId == message.From.Id);
            if (user == null)
                return;

            string answerMessage = CancelBet(user);
            await db.SaveChangesAsync();

            await client.TrySendTextMessageAsync(
                ChatId,
                answerMessage,
                replyToMessageId: message.MessageId,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
        public async Task BetColorRequest(CallbackQuery callbackQuery, ITelegramBotClient client)
        {
            DelayTimer();

            if (Resulting)
            {
                await client.TryAnswerCallbackQueryAsync(callbackQuery.Id,
                        "Барабан уже крутится, слишком поздно для ставок");
                return;
            }

            await using var db = new ChapubelichdbContext();
            User user = db.Users.FirstOrDefault(x => x.UserId == callbackQuery.From.Id);
            if (user == null)
                return;

            if (user.DefaultBet > user.Balance)
            {
                await client.TryAnswerCallbackQueryAsync(callbackQuery.Id,
                    "У тебя недостаточно средств на счету");
                return;
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

            string answerMessage = PlaceBetColor(playerChoose, user);
            await db.SaveChangesAsync();

            await client.TrySendTextMessageAsync(
                callbackQuery.Message.Chat.Id,
                answerMessage,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            await client.TryAnswerCallbackQueryAsync(callbackQuery.Id);
        }
        public async Task BetColorRequest(Message message, string pattern, ITelegramBotClient client)
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

            await using var db = new ChapubelichdbContext();
            User user = db.Users.FirstOrDefault(x => x.UserId == message.From.Id);
            if (user == null)
                return;

            if (playerBet > user.Balance)
            {
                await client.TrySendTextMessageAsync(message.Chat.Id,
                    "У тебя недостаточно средств на счету\U0001F614",
                    replyToMessageId: message.MessageId);
                return;
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

            string answerMessage = PlaceBetColor(playerChoose, user);
            await db.SaveChangesAsync();

            await client.TrySendTextMessageAsync(
                message.Chat.Id,
                answerMessage,
                replyToMessageId: message.MessageId,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

            if (!string.IsNullOrEmpty(matchString.Groups[9].Value))
                await ResultAsync(client, message);
        }
        public async Task BetNumbersRequest(CallbackQuery callbackQuery, ITelegramBotClient client)
        {
            DelayTimer();

            if (Resulting)
            {
                await client.TryAnswerCallbackQueryAsync(callbackQuery.Id,
                        "Барабан уже крутится, слишком поздно для ставок");
                return;
            }

            await using var db = new ChapubelichdbContext();
            User user = db.Users.FirstOrDefault(x => x.UserId == callbackQuery.From.Id);
            if (user == null)
                return;

            if (user.DefaultBet > user.Balance)
            {
                await client.TryAnswerCallbackQueryAsync(callbackQuery.Id,
                    "У тебя недостаточно средств на счету");
                return;
            }

            int[] userBets = Statics.RouletteGame.GetBetsByCallbackQuery(callbackQuery.Data);

            string answerMessage = PlaceBetNumber(userBets, user);
            await db.SaveChangesAsync();

            await client.TrySendTextMessageAsync(
                callbackQuery.Message.Chat.Id,
                answerMessage,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            await client.TryAnswerCallbackQueryAsync(callbackQuery.Id);
        }
        public async Task BetNumbersRequest(Message message, string pattern, ITelegramBotClient client)
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
            if (!int.TryParse(matchString.Groups[2].Value, out int firstNumber) || firstNumber > Statics.RouletteGame.TableSize)
                return;
            if (!Int32.TryParse(matchString.Groups[4].Value, out int secondNumber))
                userBets = Statics.RouletteGame.GetBetsByNumbers(firstNumber);
            else
            {
                int rangeSize = secondNumber - firstNumber + 1;

                if (firstNumber >= secondNumber || secondNumber > Statics.RouletteGame.TableSize || secondNumber == 0)
                    return;

                // Валидация ставки
                if (!(rangeSize >= 2 && rangeSize <= 4) && (rangeSize != 6 && rangeSize != 12 && rangeSize != 18))
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

                userBets = Statics.RouletteGame.GetBetsByNumbers(firstNumber, secondNumber);
            }

            await using var db = new ChapubelichdbContext();
            User user = db.Users.FirstOrDefault(x => x.UserId == message.From.Id);
            if (user == null)
                return;

            if (playerBet > user.Balance)
            {
                await client.TrySendTextMessageAsync(message.Chat.Id,
                    "У тебя недостаточно средств на счету\U0001F614",
                    replyToMessageId: message.MessageId);
                return;
            }

            string answerMessage = PlaceBetNumber(userBets, user);
            await db.SaveChangesAsync();

            await client.TrySendTextMessageAsync(
                message.Chat.Id,
                answerMessage,
                replyToMessageId: message.MessageId,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

            if (!string.IsNullOrEmpty(Regex.Match(message.Text, pattern, RegexOptions.IgnoreCase).Groups[5].Value))
                await ResultAsync(client, message);
        }
        public async Task RollRequest(CallbackQuery callbackQuery, ITelegramBotClient client)
        {
            DelayTimer();

            if (BetTokens.All(x => x.UserId != callbackQuery.From.Id))
            {
                await client.TryAnswerCallbackQueryAsync(
                callbackQuery.Id,
                "Сделай ставку, чтобы крутить барабан");
                return;
            }

            await ResultAsync(client);
            await client.TryAnswerCallbackQueryAsync(callbackQuery.Id, "✅");
        }
        public async Task RollRequest(Message message, ITelegramBotClient client)
        {
            DelayTimer();

            if (BetTokens.All(x => x.UserId != message.From.Id))
                await client.TrySendTextMessageAsync(
                message.Chat.Id,
                "Сделай ставку, чтобы крутить барабан",
                replyToMessageId: message.MessageId,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            else
                await ResultAsync(client, message);
        }
        public async Task BetInfoRequest(Message message, ITelegramBotClient client)
        {
            DelayTimer();

            await using var db = new ChapubelichdbContext();
            User user = db.Users.First(x => x.UserId == message.From.Id);
            var userTokens = BetTokens.Where(x => x.UserId == message.From.Id);

            string transactionResult = string.Empty;
            if (!userTokens.Any())
                transactionResult += $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, у тебя нет активных ставок";
            else
                transactionResult += $"Ставка <a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>:"
                                     + UserBetsToStringAsync(user);

            await client.TrySendTextMessageAsync(
                message.Chat.Id,
                transactionResult,
                replyToMessageId: message.MessageId,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
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
        private string PlaceBetColor(RouletteColorEnum playerChoose, User user)
        {
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

            return $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, ставка принята. Твоя суммарная ставка:"
                                       + UserBetsToStringAsync(user);
        }
        private string PlaceBetNumber(int[] userBets, User user)
        {
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

            return $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, ставка принята. Твоя суммарная ставка:"
                                       + UserBetsToStringAsync(user);
        }
        private string CancelBet(User user)
        {
            var userTokens = BetTokens.Where(x => x.UserId == user.UserId).ToList();
            if (userTokens.Any())
            {
                foreach (var token in userTokens)
                {
                    user.Balance += token.BetSum;
                }
                BetTokens = BetTokens.Except(userTokens).ToList();

                return $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, твоя ставка отменена \U0001F44D";
            }
            return $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, у тебя нет активных ставок";
        }
    }
}
