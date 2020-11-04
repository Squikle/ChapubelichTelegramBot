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
            await using (var db = new ChapubelichdbContext())
            {
                bool choosenGender = query.Data == "Male";

                User senderUser = db.Users.FirstOrDefault(x => x.UserId == query.From.Id);
                if (senderUser != null)
                {
                    senderUser.Gender = choosenGender;
                    await db.SaveChangesAsync();
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

                    await db.Users.AddAsync(senderUser);
                    await db.SaveChangesAsync();
                    await client.TrySendTextMessageAsync(
                        query.Message.Chat.Id,
                        "Ты был успешно зарегестрирован!",
                        replyMarkup: ReplyKeyboardsStatic.MainMarkup
                    );
                }
            }

            await client.TryDeleteMessageAsync(query.Message.Chat.Id, query.Message.MessageId);
        }
    }
}
