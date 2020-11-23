using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Managers;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = ChapubelichBot.Types.Entities.User;

namespace ChapubelichBot.CommandEntities.CallbackCommands
{
    public class GenderCallback : CallbackCommand
    {
        public override List<string> IncludingData => new List<string> {"Male", "Female"};

        public override async Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
            await using var db = new ChapubelichdbContext();
            bool choosenGender = query.Data == "Male";

            User senderUser = db.Users.FirstOrDefault(x => x.UserId == query.From.Id);

            Task deletingMessage = client.TryDeleteMessageAsync(query.Message.Chat.Id, query.Message.MessageId);

            if (senderUser != null)
                await ChangeSettings(query, db, client, senderUser, choosenGender);
            else
                await RegisterUser(query, db, client, choosenGender);

            await deletingMessage;
        }

        private async Task RegisterUser(CallbackQuery query, ChapubelichdbContext db, ITelegramBotClient client,
            bool choosenGender)
        {
            User senderUser = new User()
            {
                Gender = choosenGender,
                Username = query.From.Username,
                UserId = query.From.Id
            };

            await db.Users.AddAsync(senderUser);
            db.SaveChanges();
            await client.TrySendTextMessageAsync(
                query.Message.Chat.Id,
                "Ты был успешно зарегестрирован!",
                replyMarkup: ReplyKeyboards.MainMarkup
            );
        }

        private async Task ChangeSettings(CallbackQuery query, ChapubelichdbContext db, ITelegramBotClient client,
            User senderUser, bool choosenGender)
        {

            senderUser.Gender = choosenGender;
            db.SaveChanges();
            await client.TrySendTextMessageAsync(
                query.Message.Chat.Id,
                "Настройки успешно сохранены!",
                replyMarkup: ReplyKeyboards.SettingsMarkup);
        }
    }
}
