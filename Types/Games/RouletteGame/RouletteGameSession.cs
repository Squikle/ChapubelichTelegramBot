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
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using ChapubelichBot.Database.Models;
using ChapubelichBot.Init;
using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using Microsoft.Extensions.Configuration;

namespace ChapubelichBot.Types.Games.RouletteGame
{
    public class RouletteGameSession : IDisposable
    {
        // Getters
        public int GameMessageId => _gameSessionData.GameMessageId;
        public long ChatId => _gameSessionData.ChatId;
        public bool Resulting => _gameSessionData.Resulting;

        // Fields
        private readonly RouletteGameSessionData _gameSessionData;
        private readonly int _stopGameDelay = Bot.GetConfig().GetValue<int>("AppSettings:StopGameDelay") * 1000;
        private readonly Timer _timer;

        // C-tors
        public RouletteGameSession(long chatId, ITelegramBotClient client)
        {
            _timer = new Timer(x => DisposeAfterTime(client), null, _stopGameDelay, _stopGameDelay);
            _gameSessionData = new RouletteGameSessionData(chatId);
        }
        public RouletteGameSession(RouletteGameSessionData gameSessionData, ITelegramBotClient client)
        {
            _timer = new Timer(x => DisposeAfterTime(client), null, _stopGameDelay, _stopGameDelay);
            _gameSessionData = gameSessionData;
        }

        // Public
        public async Task Start(Message message, ITelegramBotClient client)
        {
            _gameSessionData.Resulting = false;

            _gameSessionData.ResultNumber = Statics.RouletteGame.GetRandomResultNumber();

            int replyId = message.From.Id == client.BotId ? 0 : message.MessageId;

            _gameSessionData.GameMessageId = (await client.TrySendPhotoAsync(message.Chat.Id,
                "https://i.imgur.com/SN8DRoa.png",
                caption: "Игра запущена. Ждем ваши ставки...\n" +
                         "Ты можешь поставить ставку по умолчанию на предложенные ниже варианты:",
                replyToMessageId: replyId,
                replyMarkup: InlineKeyboards.RouletteBetsMarkup)).MessageId;

            using var db = new ChapubelichdbContext();

            var dbGameSession = db.RouletteGameSessions.FirstOrDefault(x => x.ChatId == ChatId);
            if (dbGameSession == null)
            {
                db.RouletteGameSessions.Add(_gameSessionData);
                db.SaveChanges();
            }
            else
            {
                dbGameSession = _gameSessionData;
                db.SaveChanges();
            }
        }
        public async Task BetCancelRequest(CallbackQuery callbackQuery, ITelegramBotClient client)
        {
            DelayTimer();

            if (_gameSessionData.Resulting)
                return;

            await using var db = new ChapubelichdbContext();
            User user = db.Users.First(x => x.UserId == callbackQuery.From.Id);
            if (user == null)
                return;

            string answerMessage = CancelBet(user);
            db.SaveChanges();

            await client.TrySendTextMessageAsync(
                _gameSessionData.ChatId,
                answerMessage,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

            await client.TryAnswerCallbackQueryAsync(callbackQuery.Id);
        }
        public async Task BetCancelRequest(Message message, ITelegramBotClient client)
        {
            DelayTimer();

            if (_gameSessionData.Resulting)
                return;

            await using var db = new ChapubelichdbContext();
            User user = db.Users.First(x => x.UserId == message.From.Id);
            if (user == null)
                return;

            string answerMessage = CancelBet(user);
            db.SaveChanges();

            await client.TrySendTextMessageAsync(
                _gameSessionData.ChatId,
                answerMessage,
                replyToMessageId: message.MessageId,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
        public async Task BetColorRequest(CallbackQuery callbackQuery, ITelegramBotClient client)
        {
            DelayTimer();

            if (_gameSessionData.Resulting)
            {
                await client.TryAnswerCallbackQueryAsync(callbackQuery.Id,
                        "Барабан уже крутится, слишком поздно для ставок");
                return;
            }

            User user;
            await using var db = new ChapubelichdbContext();
            {
                user = db.Users.FirstOrDefault(x => x.UserId == callbackQuery.From.Id);
            }
            if (user == null)
                return;

            if (user.Balance == 0)
            {
                await client.TryAnswerCallbackQueryAsync(callbackQuery.Id,
                    "Ты не можешь сделать ставку. У тебя нет денег😞");
                return;
            }

            long playerBetSum = user.DefaultBet;
            if (playerBetSum >= user.Balance)
            {
                await client.TryAnswerCallbackQueryAsync(callbackQuery.Id,
                    "Ты ставишь все свои средства!");
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

            string answerMessage = PlaceBetColor(playerChoose, user, playerBetSum);

            await client.TrySendTextMessageAsync(
                callbackQuery.Message.Chat.Id,
                answerMessage,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            await client.TryAnswerCallbackQueryAsync(callbackQuery.Id);
        }
        public async Task BetColorRequest(Message message, string pattern, ITelegramBotClient client)
        {
            DelayTimer();

            if (_gameSessionData.Resulting)
            {
                await client.TrySendTextMessageAsync(message.Chat.Id,
                        "Барабан уже крутится, слишком поздно для ставок",
                        replyToMessageId: message.MessageId);
                return;
            }

            Match matchString = Regex.Match(message.Text, pattern, RegexOptions.IgnoreCase);

            long maxBetSum = Bot.GetConfig().GetValue<long>("AppSettings:MaxBetSum");

            if (!long.TryParse(matchString.Groups[1].Value, out long playerBetSum) || playerBetSum > maxBetSum)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    $"Вы не можете ставить больше {maxBetSum} 💵 за раз",
                    replyToMessageId: message.MessageId);
                return;
            }
            if (playerBetSum == 0)
                return;

            char betColor = matchString.Groups[2].Value.ToLower().ElementAtOrDefault(0);

            User user;
            await using (var db = new ChapubelichdbContext())
            {
               user = db.Users.FirstOrDefault(x => x.UserId == message.From.Id);
            }
            if (user == null)
                return;

            if (user.Balance == 0)
            {
                await client.TrySendTextMessageAsync(
                        message.Chat.Id,
                    $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, ты не можешь сделать ставку. У тебя нет денег😞",
                    replyToMessageId: message.MessageId,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
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

            string answerMessage = PlaceBetColor(playerChoose, user, playerBetSum);

            await client.TrySendTextMessageAsync(
                message.Chat.Id,
                answerMessage + allInAlertMessage,
                replyToMessageId: message.MessageId,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

            if (!string.IsNullOrEmpty(matchString.Groups[9].Value))
                await ResultAsync(client, message);
        }
        public async Task BetNumbersRequest(CallbackQuery callbackQuery, ITelegramBotClient client)
        {
            DelayTimer();

            if (_gameSessionData.Resulting)
            {
                await client.TryAnswerCallbackQueryAsync(callbackQuery.Id,
                        "Барабан уже крутится, слишком поздно для ставок");
                return;
            }

            User user;
            await using (var db = new ChapubelichdbContext())
            {
                user = db.Users.FirstOrDefault(x => x.UserId == callbackQuery.From.Id);
            }
            if (user == null)
                return;

            if (user.Balance == 0)
            {
                await client.TryAnswerCallbackQueryAsync(callbackQuery.Id,
                    "Ты не можешь сделать ставку. У тебя нет денег😞");
                return;
            }

            long playerBetSum = user.DefaultBet;
            if (playerBetSum >= user.Balance)
            {
                await client.TryAnswerCallbackQueryAsync(callbackQuery.Id,
                    "Ты ставишь все свои средства!");
                playerBetSum = user.Balance;
            }

            int[] userBets = Statics.RouletteGame.GetBetsByCallbackQuery(callbackQuery.Data);

            string answerMessage = PlaceBetNumber(userBets, user, playerBetSum);

            await client.TrySendTextMessageAsync(
                callbackQuery.Message.Chat.Id,
                answerMessage,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            await client.TryAnswerCallbackQueryAsync(callbackQuery.Id);
        }
        public async Task BetNumbersRequest(Message message, string pattern, ITelegramBotClient client)
        {
            DelayTimer();

            if (_gameSessionData.Resulting)
            {
                await client.TrySendTextMessageAsync(message.Chat.Id,
                        "Барабан уже крутится, слишком поздно для ставок",
                        replyToMessageId: message.MessageId);
                return;
            }

            Match matchString = Regex.Match(message.Text, pattern, RegexOptions.IgnoreCase);

            long maxBetSum = Bot.GetConfig().GetValue<long>("AppSettings:MaxBetSum");

            if (!long.TryParse(matchString.Groups[1].Value, out long playerBetSum) || playerBetSum > maxBetSum)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    $"Вы не можете ставить больше {maxBetSum} 💵 за раз",
                    replyToMessageId: message.MessageId);
                return;
            }

            if (!int.TryParse(matchString.Groups[2].Value, out int firstNumber) || firstNumber > Statics.RouletteGame.TableSize)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Некорректная ставка",
                    replyToMessageId: message.MessageId);
                return;
            }

            int[] userBets;
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

            User user;
            await using (var db = new ChapubelichdbContext())
            {
                user = db.Users.FirstOrDefault(x => x.UserId == message.From.Id);
            }
            if (user == null)
                return;

            if (user.Balance == 0)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, ты не можешь сделать ставку. У тебя нет денег😞",
                    replyToMessageId: message.MessageId,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                return;
            }

            string allInAlertMessage = string.Empty;
            if (playerBetSum >= user.Balance)
            {
                allInAlertMessage = "\n\nТы ставишь все свои средства!";
                playerBetSum = user.Balance;
            }

            string answerMessage = PlaceBetNumber(userBets, user, playerBetSum);

            await client.TrySendTextMessageAsync(
                message.Chat.Id,
                answerMessage + allInAlertMessage,
                replyToMessageId: message.MessageId,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

            if (!string.IsNullOrEmpty(Regex.Match(message.Text, pattern, RegexOptions.IgnoreCase).Groups[5].Value))
                await ResultAsync(client, message);
        }
        public async Task RollRequest(CallbackQuery callbackQuery, ITelegramBotClient client)
        {
            DelayTimer();

            if (_gameSessionData.ColorBetTokens
                    .All(x => x.UserId != callbackQuery.From.Id) 
                && _gameSessionData.NumberBetTokens
                    .All(x => x.UserId != callbackQuery.From.Id))
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

            if (_gameSessionData.ColorBetTokens
                .All(x => x.UserId != message.From.Id)
            && _gameSessionData.NumberBetTokens
                .All(x => x.UserId != message.From.Id))
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
            bool userHasTokens = _gameSessionData.ColorBetTokens
                                     .Any(x => x.UserId == message.From.Id)
                                 || _gameSessionData.NumberBetTokens
                                     .Any(x => x.UserId == message.From.Id);

            string transactionResult = string.Empty;
            if (!userHasTokens)
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
        public void Dispose()
        {
            Statics.RouletteGame.GameSessions.Remove(this);

            using var db = new ChapubelichdbContext();
            var dbGameSession = db.RouletteGameSessions.FirstOrDefault(x => x.ChatId == ChatId);
            if (dbGameSession != null)
            {
                db.RouletteGameSessions.Remove(dbGameSession);
                db.SaveChanges();
            }

            _timer.Dispose();
        }

        // Private
        private string Summarize(ChapubelichdbContext db)
        {
            StringBuilder result = new StringBuilder("Игра окончена.\nРезультат: ");
            result.Append($"{_gameSessionData.ResultNumber} {_gameSessionData.ResultNumber.ToRouletteColor().ToEmoji()}");

            List<RouletteColorBetToken> colorWinTokens = GetColorWinTokens();
            List<RouletteColorBetToken> colorLooseTokens = GetColorLooseTokens();
            List<RouletteNumbersBetToken> numberWinTokens = GetNumberWinTokens();
            List<RouletteNumbersBetToken> numberLooseTokens = GetNumberLooseTokens();

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
            if (allLooseTokens.Any())
            {
                result.Append("\n\U0001F614<b>Проиграли:</b>");
                foreach (var token in allLooseTokens.GroupByUsers())
                {
                    User user = db.Users.FirstOrDefault(x => x.UserId == token.UserId);
                    if (user != null)
                        result.Append(
                            $"\n<b>·</b><a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>: <b>-{token.BetSum.ToMoneyFormat()}</b>💵");
                }
            }

            return result.ToString();
        }
        private async Task ResultAsync(ITelegramBotClient client, Message startMessage = null)
        {
            using var db = new ChapubelichdbContext();
            if (_gameSessionData.Resulting)
                return;

            _gameSessionData.Resulting = true;

            var dbGameSession = db.RouletteGameSessions.FirstOrDefault(x => x.ChatId == ChatId);
            _gameSessionData.AnimationMessageId = (await client.TrySendAnimationAsync(_gameSessionData.ChatId,
                GetRandomAnimationLink(), disableNotification: true, caption: "Крутим барабан...")).MessageId;
            if (dbGameSession != null)
            {
                dbGameSession.Resulting = true;
                dbGameSession.AnimationMessageId = _gameSessionData.AnimationMessageId;

                db.SaveChanges();
            }

            int configAnimationDuration = Bot.GetConfig().GetValue<int>("AppSettings:RouletteAnimationDuration") * 1000;
            Task task = Task.Delay(configAnimationDuration >= 10 * 1000 ? 10000 : configAnimationDuration);

            // Удаление сообщений и отправка результатов
            string result = Summarize(db);
            await task;

            if (_gameSessionData.AnimationMessageId != 0)
                await client.TryDeleteMessageAsync(ChatId, _gameSessionData.AnimationMessageId);

            if (_gameSessionData.GameMessageId != 0)
                await client.TryDeleteMessageAsync(ChatId, _gameSessionData.GameMessageId);

            int replyId = startMessage?.MessageId ?? 0;
            await client.TrySendTextMessageAsync(
                _gameSessionData.ChatId,
                result,
                Telegram.Bot.Types.Enums.ParseMode.Html,
                replyMarkup: InlineKeyboards.RoulettePlayAgainMarkup,
                replyToMessageId: replyId);

            db.SaveChanges();
            Dispose();
        }
        public async Task ResumeResultingAsync(ITelegramBotClient client)
        {
            using var db = new ChapubelichdbContext();
            string result = Summarize(db);

            if (_gameSessionData.GameMessageId != 0)
                await client.TryDeleteMessageAsync(ChatId, _gameSessionData.GameMessageId);

            if (_gameSessionData.AnimationMessageId != 0)
                await client.TryDeleteMessageAsync(ChatId, _gameSessionData.AnimationMessageId);

            await client.TrySendTextMessageAsync(
                _gameSessionData.ChatId,
                result,
                Telegram.Bot.Types.Enums.ParseMode.Html,
                replyMarkup: InlineKeyboards.RoulettePlayAgainMarkup);
            db.SaveChanges();
            Dispose();
        }
        private StringBuilder UserBetsToStringAsync(User user)
        {
            StringBuilder resultList = new StringBuilder();
            var userColorTokens = _gameSessionData.ColorBetTokens.Where(x => x.UserId == user.UserId).ToList();
            var userNumberTokens = _gameSessionData.NumberBetTokens.Where(x => x.UserId == user.UserId).ToList();

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
        private List<RouletteColorBetToken> GetColorWinTokens()
        {
            return _gameSessionData.ColorBetTokens
                .Where(x => x.ChoosenColor == _gameSessionData.ResultNumber.ToRouletteColor()).ToList();
        }
        private List<RouletteNumbersBetToken> GetNumberWinTokens()
        {
            return _gameSessionData.NumberBetTokens
                .Where(x => x.ChoosenNumbers
                    .Contains(_gameSessionData.ResultNumber)).ToList();
        }
        private List<RouletteColorBetToken> GetColorLooseTokens()
        {
            return _gameSessionData.ColorBetTokens.Where(x => x.ChoosenColor != _gameSessionData.ResultNumber.ToRouletteColor()).ToList();
        }
        private List<RouletteNumbersBetToken> GetNumberLooseTokens()
        {
            return _gameSessionData.NumberBetTokens.Where(x => !x.ChoosenNumbers.Contains(_gameSessionData.ResultNumber)).ToList();
        }
        private async void DisposeAfterTime(ITelegramBotClient client)
        {
            if (_gameSessionData.Resulting)
                return;

            var returnedBets = string.Empty;

            await using (var db = new ChapubelichdbContext())
            {
                _gameSessionData.Resulting = true;

                var dbGameSession = db.RouletteGameSessions.FirstOrDefault(x => x.ChatId == ChatId);
                if (dbGameSession != null)
                {
                    dbGameSession.Resulting = true;
                    db.SaveChanges();
                }

                var groupedColorBetList = _gameSessionData.ColorBetTokens.GroupByUsers().ToList();
                var groupedNumberBetList = _gameSessionData.NumberBetTokens.GroupByUsers().ToList();
                if (groupedColorBetList.Any() || groupedNumberBetList.Any())
                {
                    foreach (var bet in groupedColorBetList)
                    {
                        var user = db.Users.FirstOrDefault(x => x.UserId == bet.UserId);
                        if (user != null)
                            user.Balance += bet.BetSum;
                    }
                    foreach (var bet in groupedNumberBetList)
                    {
                        var user = db.Users.FirstOrDefault(x => x.UserId == bet.UserId);
                        if (user != null)
                            user.Balance += bet.BetSum;
                    }
                    returnedBets += "\nСтавки были возвращены👍";
                }
                db.SaveChanges();
            }

            if (_gameSessionData.GameMessageId != 0)
            {
                await client.TryDeleteMessageAsync(_gameSessionData.GameMessageId, _gameSessionData.GameMessageId);
            }
            await client.TrySendTextMessageAsync(
                _gameSessionData.ChatId,
                "Игровая сессия отменена из-за отсутствия активности" + returnedBets,
                Telegram.Bot.Types.Enums.ParseMode.Html,
                replyMarkup: InlineKeyboards.RoulettePlayAgainMarkup);

            Dispose();
        }
        private void DelayTimer()
        {
            _timer.Change(_stopGameDelay, _stopGameDelay);
        }

        // Static
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
        private string PlaceBetColor(RouletteColorEnum playerChoose, User user, long betSum)
        {
            using (var db = new ChapubelichdbContext())
            {
                var colorBetTokens = _gameSessionData.ColorBetTokens;
                RouletteColorBetToken currentBetToken =
                    colorBetTokens.FirstOrDefault(x => x.ChoosenColor == playerChoose && x.UserId == user.UserId);

                var dbGameSession = db.RouletteGameSessions.FirstOrDefault(x => x.ChatId == ChatId);

                if (currentBetToken != null)
                {
                    currentBetToken.BetSum += betSum;

                    if (dbGameSession != null)
                    {
                        var dbCurrentBetToken = dbGameSession.ColorBetTokens
                            .FirstOrDefault(x => x.ChoosenColor == playerChoose && x.UserId == user.UserId);
                        if (dbCurrentBetToken != null)
                            dbCurrentBetToken.BetSum += betSum;
                    }
                    else
                        db.RouletteGameSessions.Add(_gameSessionData);
                }
                else
                {
                    currentBetToken = new RouletteColorBetToken(user, betSum, playerChoose);
                    _gameSessionData.ColorBetTokens.Add(currentBetToken);

                    if (dbGameSession != null)
                        dbGameSession.ColorBetTokens.Add(currentBetToken);
                    else
                        db.RouletteGameSessions.Add(_gameSessionData);
                }

                db.Attach(user);
                user.Balance -= betSum;
                db.SaveChanges();
            }

            return $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, ставка принята. Твоя суммарная ставка:"
                   + UserBetsToStringAsync(user);
        }
        private string PlaceBetNumber(int[] userBets, User user, long betSum)
        {
            using (var db = new ChapubelichdbContext())
            {
                var numberBetTokens = _gameSessionData.NumberBetTokens;
                RouletteNumbersBetToken currentBetToken = numberBetTokens
                    .FirstOrDefault(x =>
                        x.ChoosenNumbers.SequenceEqual(userBets) && x.UserId == user.UserId);

                var dbGameSession = db.RouletteGameSessions.FirstOrDefault(x => x.ChatId == ChatId);

                if (currentBetToken != null)
                {
                    currentBetToken.BetSum += betSum;

                    if (dbGameSession != null)
                    {
                        var dbCurrentBetToken = dbGameSession.NumberBetTokens
                            .FirstOrDefault(x =>
                                x.ChoosenNumbers.SequenceEqual(userBets) && x.UserId == user.UserId);
                        if (dbCurrentBetToken != null)
                            dbCurrentBetToken.BetSum += betSum;
                    }
                    else
                        db.RouletteGameSessions.Add(_gameSessionData);
                }
                else
                {
                    currentBetToken = new RouletteNumbersBetToken(user, betSum, userBets);
                    _gameSessionData.NumberBetTokens.Add(currentBetToken);

                    if (dbGameSession != null)
                    {
                        dbGameSession.NumberBetTokens.Add(currentBetToken);
                    }
                    else
                    {
                        db.RouletteGameSessions.Add(_gameSessionData);
                    }
                }

                db.Attach(user);
                user.Balance -= betSum;
                db.SaveChanges();
            }

            return $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, ставка принята. Твоя суммарная ставка:"
                   + UserBetsToStringAsync(user);
        }
        private string CancelBet(User user)
        {
            var userColorTokens = _gameSessionData.ColorBetTokens.Where(x => x.UserId == user.UserId).ToList();
            var userNumberTokens = _gameSessionData.NumberBetTokens.Where(x => x.UserId == user.UserId).ToList();

            if (userColorTokens.Any() || userNumberTokens.Any())
            {
                using (var db = new ChapubelichdbContext())
                {
                    foreach (var token in userColorTokens)
                    {
                        user.Balance += token.BetSum;
                        _gameSessionData.ColorBetTokens.Remove(token);
                    }

                    foreach (var token in userNumberTokens)
                    {
                        user.Balance += token.BetSum;
                        _gameSessionData.NumberBetTokens.Remove(token);
                    }

                    var dbGameSession = db.RouletteGameSessions.FirstOrDefault(x => x.ChatId == ChatId);
                    if (dbGameSession != null)
                    {
                        dbGameSession.ColorBetTokens = _gameSessionData.ColorBetTokens;
                        dbGameSession.NumberBetTokens = _gameSessionData.NumberBetTokens;
                    }
                    else
                    {
                        db.RouletteGameSessions.Add(_gameSessionData);
                    }
                    db.SaveChanges();
                }
                return $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, твоя ставка отменена \U0001F44D";
            }
            return $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, у тебя нет активных ставок";
        }
    }
}
