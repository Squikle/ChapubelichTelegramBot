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

namespace Chapubelich.Chatting.RegexCommands
{
    class RouletteNumberBetRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *(\d{1,4}) +([0-9]|[1-3][0-9])( *- *([1-9]|[1-3][0-9]))? *(го|ролл|погнали|крути|roll|go)?(@ChapubelichBot)?$";

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
                if (firstNumber >= secondNumber || secondNumber > RouletteTableStatic.tableSize)
                    return;
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

                RouletteBetToken currentBetToken = gameSession.BetTokens.FirstOrDefault(x => x.ChoosenNumbers.SequenceEqual(userBets) && x.UserId == user.UserId);

                if (null != currentBetToken)
                    currentBetToken.BetSum += playerBet;
                else
                {
                    currentBetToken = new RouletteBetToken(user, playerBet, choosenNumbers: userBets);
                    gameSession.BetTokens.Add(currentBetToken);
                }

                user.Balance -= playerBet;
                await db.SaveChangesAsync();

                string transactionResult = $"Ставка принята. Ставка <a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>:"
                    + gameSession.UserBetsToString(user);

                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    transactionResult,
                    replyToMessageId: message.MessageId,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

                if (!string.IsNullOrEmpty(Regex.Match(message.Text, Pattern, RegexOptions.IgnoreCase).Groups[5].Value))
                    gameSession.Result(client, message);
            }

        }
    }
}
