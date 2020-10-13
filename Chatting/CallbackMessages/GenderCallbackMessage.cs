using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using ChapubelichBot.Database;
using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Statics;
using User = ChapubelichBot.Database.Models.User;

namespace ChapubelichBot.Chatting.CallbackMessages
{
    public class GenderCallbackMessage : CallBackMessage
    {
        public override List<string> IncludingData => new List<string> { "Male", "Female" };
        public override async Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
            using (var db = new ChapubelichdbContext())
            {
                bool choosenGender = false;

                switch (query.Data)
                {
                    case "Male":
                        choosenGender = true;
                        break;
                    case "Female":
                        choosenGender = false;
                        break;
                }

                User senderUser = db.Users.FirstOrDefault(x => x.UserId == query.From.Id);
                if (senderUser != null)
                {
                    senderUser.Gender = choosenGender;
                    db.SaveChanges();
                    await client.TryDeleteMessageAsync(
                        query.Message.Chat.Id,
                        query.Message.MessageId);
                    await client.TrySendTextMessageAsync(
                        query.Message.Chat.Id,
                        "Настройки успешно сохранены!",
                        replyMarkup: ReplyKeyboardsStatic.SettingsMarkup
                        );
                    return;
                }
                else
                {
                    senderUser = new User()
                    {
                        Gender = choosenGender,
                        Username = query.From.Username,
                        UserId = query.From.Id,
                        FirstName = query.From.FirstName
                    };

                    db.Users.Add(senderUser);
                    db.SaveChanges();
                    await client.TrySendTextMessageAsync(
                        query.Message.Chat.Id,
                        "Вы были успешно зарегестрированы!",
                        replyMarkup: ReplyKeyboardsStatic.MainMarkup
                        );
                }
            }

            await client.TryDeleteMessageAsync(query.Message.Chat.Id, query.Message.MessageId);

            return;
        }
    }
}
