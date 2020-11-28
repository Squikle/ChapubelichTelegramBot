using System;
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

            string result;
            if (senderUser != null)
                result = await ChangeSettingsAsync(dbContext, senderUser, choosenGender);
            else
                result = await RegisterUserAsync(query, dbContext, choosenGender);

            if (!string.IsNullOrEmpty(result))
                await client.TryEditMessageAsync(query.Message.Chat.Id, query.Message.MessageId, result);
        }

        private async Task<string> RegisterUserAsync(CallbackQuery query, ChapubelichdbContext dbContext,
            bool choosenGender)
        {
            User senderUser = new User
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
                Console.WriteLine("Повторное добавление юзера");
                return null;
            }

            return "Ты был успешно зарегестрирован!";
        }

        private async Task<string> ChangeSettingsAsync(ChapubelichdbContext dbContext,
            User senderUser, bool choosenGender)
        {
            senderUser.Gender = choosenGender;
            await dbContext.SaveChangesAsync();
            return "Настройки успешно сохранены!";
        }
    }
}
