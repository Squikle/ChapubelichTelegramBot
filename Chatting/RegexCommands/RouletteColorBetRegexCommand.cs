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
    class RouletteColorBetRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/?(\d{1,4}) ?(к(расный)?|ч(ерный)?|з(еленый)?|r(ed)?|b(lack)?|g(reen)?) *(го|ролл|погнали|крути|roll|go)?(@ChapubelichBot)?$";

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

                var colorBetTokens = gameSession.BetTokens.OfType<RouletteColorBetToken>();
                RouletteColorBetToken currentBetToken = colorBetTokens.FirstOrDefault(x => x.ChoosenColor == playerChoose && x.UserId == user.UserId);

                if (null != currentBetToken)
                    currentBetToken.BetSum += playerBet;
                else
                {
                    currentBetToken = new RouletteColorBetToken(user, playerBet, playerChoose);
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

                if (!string.IsNullOrEmpty(matchString.Groups[9].Value))
                    gameSession.ResultAsync(client, message);
            }
        }
    }
}
