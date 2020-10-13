using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using ChapubelichBot.Database;
using ChapubelichBot.Types.Extensions;
using System.Threading.Tasks;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = ChapubelichBot.Database.Models.User;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteCancelRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *(отмена|cancel)(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            var gameSession = RouletteTableStatic.GetGameSessionByChatId(message.Chat.Id);
            if (gameSession == null || gameSession.Resulting)
                return;

            using (var db = new ChapubelichdbContext())
            {
                User user = db.Users.First(x => x.UserId == message.From.Id);
                if (user == null)
                    return;
                var userTokens = gameSession.BetTokens.Where(x => x.UserId == message.From.Id);
                if (userTokens.Any())
                {
                    foreach (var token in userTokens)
                    {
                        user.Balance += token.BetSum;
                    }
                    gameSession.BetTokens = gameSession.BetTokens.Except(userTokens).ToList();

                    db.SaveChanges();

                    await client.TrySendTextMessageAsync(
                            gameSession.ChatId,
                            $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, Ваша ставка отменена \U0001F44D",
                            replyToMessageId: message.MessageId,
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                }

                else await client.TrySendTextMessageAsync(
                            gameSession.ChatId,
                            $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, у Вас нет активных ставок",
                            replyToMessageId: message.MessageId,
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }
        }
    }
}
