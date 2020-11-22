using System.Linq;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.Commands
{
    class ProfileInfo : Command
    {
        public override string Name => "👤 Мой профиль";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await using var db = new ChapubelichdbContext();
            var user = db.Users.FirstOrDefault(x => x.UserId == message.From.Id);

            if (user == null)
                return;

            string gender = user.Gender ? "мужской" : "женский";

            string answerMessage = 
                $"<a href=\"tg://user?id={user.UserId}\">{message.From.FirstName}</a>, твой профиль:\n" +
                $"Пол: {gender}\n" +
                $"Баланс: {user.Balance.ToMoneyFormat()}\n" +
                $"Id: {user.UserId}\n" +
                $"Ставка по умолчанию: {user.DefaultBet}";

            await client.TrySendTextMessageAsync(message.From.Id, answerMessage, 
                replyToMessageId: message.MessageId, 
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, 
                replyMarkup: ReplyKeyboards.SettingsMarkup);
        }
    }
}
