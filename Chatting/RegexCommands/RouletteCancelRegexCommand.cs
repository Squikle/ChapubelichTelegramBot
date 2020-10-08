using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using ChapubelichBot.Database;
using ChapubelichBot.Types.Extensions;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = ChapubelichBot.Database.Models.User;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteCancelRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *(отмена|cancel)(@ChapubelichBot)?$";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            var gameSession = RouletteGameStatic.GetGameSessionByChatId(message.Chat.Id);
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
