using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Managers;
using ChapubelichBot.Types.Managers.MessagesSender;
using ChapubelichBot.Types.Statics;
using Microsoft.EntityFrameworkCore;
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
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            bool choosenGender = query.Data == "Male";

            User senderUser = await dbContext.Users.FirstOrDefaultAsync(x => x.UserId == query.From.Id);

            Task deletingMessage = client.TryDeleteMessageAsync(query.Message.Chat.Id, query.Message.MessageId);

            if (senderUser != null)
                await ChangeSettingsAsync(query, dbContext, client, senderUser, choosenGender);
            else
                await RegisterUserAsync(query, dbContext, client, choosenGender);

            await deletingMessage;
        }

        private async Task RegisterUserAsync(CallbackQuery query, ChapubelichdbContext dbContext, ITelegramBotClient client,
            bool choosenGender)
        {
            User senderUser = new User()
            {
                Gender = choosenGender,
                Username = query.From.Username,
                UserId = query.From.Id
            };

            await dbContext.Users.AddAsync(senderUser);
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return;
            }
            await client.TrySendTextMessageAsync(
                query.Message.Chat.Id,
                "Ты был успешно зарегестрирован!",
                replyMarkup: ReplyKeyboards.MainMarkup
            );
        }

        private async Task ChangeSettingsAsync(CallbackQuery query, ChapubelichdbContext dbContext, ITelegramBotClient client,
            User senderUser, bool choosenGender)
        {

            senderUser.Gender = choosenGender;
            await dbContext.SaveChangesAsync();
            await client.TrySendTextMessageAsync(
                query.Message.Chat.Id,
                "Настройки успешно сохранены!",
                replyMarkup: ReplyKeyboards.SettingsMarkup);
        }
    }
}
