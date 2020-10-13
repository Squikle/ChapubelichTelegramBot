using ChapubelichBot.Database;
using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Games.RouletteGame;
using ChapubelichBot.Types.Statics;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = ChapubelichBot.Database.Models.User;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteNumberBetRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *(\d{1,4}) +([0-9]|[1-3][0-9])( *- *([0-9]|[1-3][0-9]))? *(го|ролл|погнали|крути|roll|go)?(@ChapubelichBot)?$";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            var gameSession = RouletteTableStatic.GetGameSessionByChatId(message.Chat.Id);

            if (null == gameSession)
                return;
            if (gameSession.Resulting)
            {
                await client.TrySendTextMessageAsync(message.Chat.Id,
                        "Барабан уже крутится, слишком поздно для ставок",
                        replyToMessageId: message.MessageId);
                return;
            }

            Match matchString = Regex.Match(message.Text, Pattern, RegexOptions.IgnoreCase);

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

                var numberBetTokens = gameSession.BetTokens.OfType<RouletteNumbersBetToken>();
                RouletteNumbersBetToken currentBetToken = numberBetTokens.FirstOrDefault(x => x.ChoosenNumbers.SequenceEqual(userBets) && x.UserId == user.UserId);

                if (currentBetToken != null)
                    currentBetToken.BetSum += playerBet;
                else
                {
                    currentBetToken = new RouletteNumbersBetToken(user, playerBet, userBets);
                    gameSession.BetTokens.Add(currentBetToken);
                }

                user.Balance -= playerBet;
                db.SaveChanges();

                string transactionResult = $"Ставка принята. Суммарная ставка <a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>:"
                    + gameSession.UserBetsToStringAsync(user);

                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    transactionResult,
                    replyToMessageId: message.MessageId,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

                if (!string.IsNullOrEmpty(Regex.Match(message.Text, Pattern, RegexOptions.IgnoreCase).Groups[5].Value))
                    gameSession.ResultAsync(client, message);
            }
        }
    }
}
