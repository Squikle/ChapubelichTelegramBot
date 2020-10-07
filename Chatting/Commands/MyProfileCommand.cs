using Chapubelich.Abstractions;
using Chapubelich.ChapubelichBot.Statics;
using Chapubelich.Database;
using Chapubelich.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Chapubelich.Chatting.Commands
{
    class MyProfileCommand : Command
    {
        public override string Name => "\U0001F464 Мой профиль";

        public override async void Execute(Message message, ITelegramBotClient client)
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
                $"Баланс: {user.Balance}\n" +
                $"Id: {user.UserId}";

                await client.TrySendTextMessageAsync(message.From.Id, answerMessage, 
                    replyToMessageId: message.MessageId, 
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, 
                    replyMarkup: ReplyKeyboards.SettingsMarkup);
            }
        }
    }
}
