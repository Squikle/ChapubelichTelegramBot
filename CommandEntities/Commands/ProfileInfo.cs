using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Managers.MessagesSender;
using ChapubelichBot.Types.Statics;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.Commands
{
    class ProfileInfo : Command
    {
        public override string Name => "👤 Мой профиль";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            var user = await dbContext.Users
                .Include(u => u.UserCompliment)
                .FirstOrDefaultAsync(x => x.UserId == message.From.Id);

            if (user == null)
                return;

            string gender = user.Gender ? "Мужской" : "Женский";
            string complimentSubscription = user.UserCompliment != null ? "Да" : "Нет";
 
            string answerMessage = 
                $"<i><a href=\"tg://user?id={user.UserId}\">{message.From.FirstName}</a></i>, твой профиль:\n" +
                $"Пол: <b>{gender}</b>\n" +
                $"Баланс: <b>{user.Balance.ToMoneyFormat()}</b>\n" +
                $"Id: <b>{user.UserId}</b>\n" +
                $"Ставка по умолчанию: <b>{user.DefaultBet}</b>\n" +
                $"Подписка на комлпименты: <b>{complimentSubscription}</b>";

            await client.TrySendTextMessageAsync(message.From.Id, answerMessage, 
                replyToMessageId: message.MessageId, 
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, 
                replyMarkup: ReplyKeyboards.SettingsMarkup);
        }
    }
}
