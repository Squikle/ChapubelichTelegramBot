using Chapubelich.Abstractions;
using Chapubelich.Database;
using Chapubelich.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Game = Chapubelich.Abstractions.Game;
using User = Chapubelich.Database.Models.User;

namespace Chapubelich.Chatting.Games.FiftyFiftyGame
{
    class FiftyFiftyCheckBetCommand : RegexCommand
    {
        public override string Pattern => @"^ *\/?((мо(и|я))|(my))?\s*((ставк(и|а))|(bets?))(@ChapubelichBot)?$";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            var gameSession = Game.GetGameSession("\U0001F3B0 50/50", message.Chat.Id);
            if (null == gameSession)
                return;

            using (var db = new ChapubelichdbContext())
            {
                User user = db.Users.First(x => x.UserId == message.From.Id);
                var userTokens = gameSession.BetTokens.Where(x => x.UserId == message.From.Id);

                string transactionResult = string.Empty;
                if (!userTokens.Any())
                    transactionResult += "\nУ вас нет текущих ставок";
                else
                {
                    transactionResult += $"Ставка <a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>:";
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
