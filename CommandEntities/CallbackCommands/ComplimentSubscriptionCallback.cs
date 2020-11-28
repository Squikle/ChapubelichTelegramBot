using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Entities;
using ChapubelichBot.Types.Managers.MessagesSender;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.CallbackCommands
{
    class ComplimentSubscriptionCallback : CallbackCommand
    {
        public override List<string> IncludingData => new List<string> { "EnableCompliments", "DisableCompliments" };
        public override async Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            var user = await dbContext.Users
                .Include(u => u.UserCompliment)
                .FirstOrDefaultAsync(u => u.UserId == query.From.Id);

            if (user == null)
                return;

            string resultMessage;
            if (query.Data == "EnableCompliments")
            {
                if (user.UserCompliment != null)
                {
                    resultMessage = "Ты уже подписан на ежедневные комлименты 😉";
                }
                else
                {
                    user.UserCompliment = new UserCompliment
                    {
                        Praised = false,
                        User = user
                    };
                    try
                    {
                        await dbContext.SaveChangesAsync();
                    }
                    catch (DbUpdateException)
                    {
                        Console.WriteLine("Повторная подписка на комплименты");
                        return;
                    }

                    resultMessage = "Ты успешно подписался на ежедневные комлименты	💚";
                }
            }
            else
            {
                if (user.UserCompliment == null)
                {
                    resultMessage = "Ты и так не подписан на ежедневные комплименты 😢";
                }
                else
                {
                    dbContext.Remove(user.UserCompliment);
                    try
                    {
                        await dbContext.SaveChangesAsync();
                    }
                    catch (DbUpdateException)
                    {
                        Console.WriteLine("Повторное удаление подписки на комплименты");
                        return;
                    }

                    resultMessage = "Ты успешно отписался от ежедневных комплиментов 💔";
                }
            }

            await client.TryEditMessageAsync(query.Message.Chat.Id, query.Message.MessageId, resultMessage);
        }
    }
}
