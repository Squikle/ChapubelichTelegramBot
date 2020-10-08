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
    class RouletteCheckBetRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *((мо(и|я))|(my))?\s*((ставк(и|а))|(bets?))(@ChapubelichBot)?$";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            var gameSession = RouletteTableStatic.GetGameSessionByChatId(message.Chat.Id);
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
                    transactionResult += $"Ставка <a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>:" 
                        + gameSession.UserBetsToString(user);
                    
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    transactionResult,
                    replyToMessageId: message.MessageId,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }
        }
    }
}