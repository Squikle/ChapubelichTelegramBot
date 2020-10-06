using Chapubelich.Abstractions;
using Chapubelich.Database;
using Chapubelich.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Game = Chapubelich.Abstractions.Game;
using User = Chapubelich.Database.Models.User;

namespace Chapubelich.Chatting.Games.FiftyFiftyGame
{
    class FiftyFiftyCancelRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *(отмена|cancel)(@ChapubelichBot)?$";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            var gameSession = FiftyFiftyGame.GetGameSessionByChatId(message.Chat.Id);
            if (null != gameSession)
            {
                using (var db = new ChapubelichdbContext())
                {
                    User user = db.Users.First(x => x.UserId == message.From.Id);
                    var userTokens = gameSession.BetTokens.Where(x => x.UserId == message.From.Id);
                    foreach (var token in userTokens)
                    {
                        user.Balance += token.BetSum;
                    }
                    gameSession.BetTokens = gameSession.BetTokens.Except(userTokens).ToList();

                    db.SaveChanges();
                }

                await client.TrySendTextMessageAsync(
                        message.Chat.Id,
                        "Ваша ставка отменена \U0001F44D",
                        replyToMessageId: message.MessageId,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }
        }
    }
}
