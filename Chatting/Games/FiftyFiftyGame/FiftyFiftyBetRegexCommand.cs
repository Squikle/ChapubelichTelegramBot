using Chapubelich.Abstractions;
using Chapubelich.ChapubelichBot.LocalModels;
using Chapubelich.Database;
using Chapubelich.Extensions;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Game = Chapubelich.Abstractions.Game;
using User = Chapubelich.Database.Models.User;

namespace Chapubelich.Chatting.Games.FiftyFiftyGame
{
    class FiftyFiftyBetRegexCommand : RegexCommand
    {
        public override string Pattern => @"(^ *\/?\d{1,6}) ?(к|красный|ч|черный)(@ChapubelichBot)?$";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            var gameSession = Game.GetGameSession("\U0001F3B0 50/50", message.Chat.Id);

            if (null == gameSession)
                return;

            string bet = Regex.Match(message.Text, Pattern, RegexOptions.IgnoreCase).Groups[1].Value;
            string choose = Regex.Match(message.Text, Pattern, RegexOptions.IgnoreCase).Groups[2].Value.ToLower();

            if (null == bet || null == choose)
                return;

            // Определение ставки игрока
            int playerBet = Convert.ToInt32(bet);
            int playerChoose = 0;

            switch (choose[0])
            {
                case 'к':
                    playerChoose = 0;
                    break;
                case 'ч':
                    playerChoose = 1;
                    break;
            }

            using (var db = new ChapubelichdbContext())
            {
                User user = db.Users.First(x => x.UserId == message.From.Id);
                if (null == user)
                    return;

                BetToken currentBetToken = gameSession.BetTokens.FirstOrDefault(x => x.Choose == playerChoose && x.UserId == user.UserId);

                if (null != currentBetToken)
                    currentBetToken.BetSum += playerBet;
                else
                {
                    currentBetToken = new BetToken(user, playerBet, playerChoose);
                    gameSession.BetTokens.Add(currentBetToken);
                }

                user.Balance -= playerBet;
                await db.SaveChangesAsync();


                string transactionResult = $"Ставка принята. Ставка <a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>:";
                var userTokens = gameSession.BetTokens.Where(x => x.UserId == user.UserId);
                foreach (var token in userTokens)
                {
                    switch (token.Choose)
                    {
                        case 0:
                            transactionResult += $"<b>\n\U0001F534 {token.BetSum}</b>";
                            break;
                        case 1:
                            transactionResult += $"<b>\n\U000026AB {token.BetSum}</b>";
                            break;
                    }
                }
                                
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    transactionResult,
                    replyToMessageId: message.MessageId,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }
        }
    }
}
