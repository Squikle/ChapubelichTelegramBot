using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using ChapubelichBot.Database;
using ChapubelichBot.Types.Extensions;
using System.Threading.Tasks;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.Commands
{
    class MyProfileCommand : Command
    {
        public override string Name => "\U0001F464 Мой профиль";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            using (var db = new ChapubelichdbContext())
            {
                var user = db.Users.FirstOrDefault(x => x.UserId == message.From.Id);

                if (user == null)
                    return;

                string gender = user.Gender ? "мужской" : "женский";

                string answerMessage = 
                $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, Ваш профиль:\n" +
                $"Имя: {user.FirstName}\n" +
                $"Пол: {gender}\n" +
                $"Баланс: {user.Balance.ToMoneyFormat()}\n" +
                $"Id: {user.UserId}\n" +
                $"Ставка по умолчанию: {user.DefaultBet}";

                await client.TrySendTextMessageAsync(message.From.Id, answerMessage, 
                    replyToMessageId: message.MessageId, 
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, 
                    replyMarkup: ReplyKeyboardsStatic.SettingsMarkup);
            }
        }
    }
}
