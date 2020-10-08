using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using ChapubelichBot.Database;
using ChapubelichBot.Types.Extensions;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = ChapubelichBot.Database.Models.User;
using ChapubelichBot.Types.Enums;
using ChapubelichBot.Types.Games.RouletteGame;
using System.Collections.Generic;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteBetRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/?(\d{1,4}) ?(к(расный)?|ч(ерный)?|з(еленый)?|r(ed)?|b(lack)?|g(reen)?) *(го|ролл|погнали|крути|roll|go)?(@ChapubelichBot)?$";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            var gameSession = RouletteGameStatic.GetGameSessionByChatId(message.Chat.Id);

            if (null == gameSession)
                return;
            if (gameSession.Rolling)
            {
                await client.TrySendTextMessageAsync(message.Chat.Id,
                        "Барабан уже крутится, слишком поздно для ставок",
                        replyToMessageId: message.MessageId);
                return;
            }

            string betString = Regex.Match(message.Text, Pattern, RegexOptions.IgnoreCase).Groups[1].Value;
            string chooseString = Regex.Match(message.Text, Pattern, RegexOptions.IgnoreCase).Groups[2].Value.ToLower();

            // Определение ставки игрока
            RouletteColorEnum playerChoose = RouletteColorEnum.Red;

            if (!Int32.TryParse(betString, out int playerBet) || chooseString == null || playerBet == 0)
                return;

            if (chooseString[0] == 'к' || chooseString[0] == 'r')
                playerChoose = RouletteColorEnum.Red;
            else if (chooseString[0] == 'ч' || chooseString[0] == 'b')
                playerChoose = RouletteColorEnum.Black;
            else if (chooseString[0] == 'з' || chooseString[0] == 'g')
                playerChoose = RouletteColorEnum.Green;

            using (var db = new ChapubelichdbContext())
            {
                User user = db.Users.First(x => x.UserId == message.From.Id);

                if (playerBet > user.Balance)
                {
                    await client.TrySendTextMessageAsync(message.Chat.Id,
                        "У вас недостаточно средств на счету\U0001F614",
                        replyToMessageId: message.MessageId);
                    return;
                }

                RouletteBetToken currentBetToken = gameSession.BetTokens.FirstOrDefault(x => x.ColorChoose == playerChoose && x.UserId == user.UserId);

                if (null != currentBetToken)
                    currentBetToken.BetSum += playerBet;
                else
                {
                    currentBetToken = new RouletteBetToken(user, playerBet, playerChoose);
                    gameSession.BetTokens.Add(currentBetToken);
                }

                user.Balance -= playerBet;
                await db.SaveChangesAsync();

                string transactionResult = $"Ставка принята. Ставка <a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>:";
                transactionResult += RouletteGameStatic.GetUserListOfBets(user, gameSession);
                                
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    transactionResult,
                    replyToMessageId: message.MessageId,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

                if (!string.IsNullOrEmpty(Regex.Match(message.Text, Pattern, RegexOptions.IgnoreCase).Groups[9].Value))
                    gameSession.Result(client, message);
            }
        }
    }
}
