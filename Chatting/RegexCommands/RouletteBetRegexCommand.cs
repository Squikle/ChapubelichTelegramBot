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

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteBetRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/?(\d{1,4}) ?(к(расный)?|ч(ерный)?|з(еленый)?|r(ed)?|b(lack)?|g(reen)?)(@ChapubelichBot)?$";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            var gameSession = RouletteGameStatic.GetGameSessionByChatId(message.Chat.Id);

            if (null == gameSession)
                return;

            string betString = Regex.Match(message.Text, Pattern, RegexOptions.IgnoreCase).Groups[1].Value;
            string chooseString = Regex.Match(message.Text, Pattern, RegexOptions.IgnoreCase).Groups[2].Value.ToLower();

            // Определение ставки игрока
            RouletteColorEnum playerChoose = RouletteColorEnum.Red;

            if (!Int32.TryParse(betString, out int playerBet) || chooseString == null)
                return;

            switch (chooseString[0])
            {
                case 'к':
                    playerChoose = RouletteColorEnum.Red;
                    break;
                case 'ч':
                    playerChoose = RouletteColorEnum.Black;
                    break;
                case 'з':
                    playerChoose = RouletteColorEnum.Green;
                    break;
            }

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
            }
        }
    }
}
