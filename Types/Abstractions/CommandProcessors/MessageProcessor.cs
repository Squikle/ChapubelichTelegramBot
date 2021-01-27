using System;
using System.Linq;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Entities.Groups;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Types.Abstractions.CommandProcessors
{
    public abstract class MessageProcessor
    {
        protected static async Task<Group> UpdateGroupAsync(Message message, ITelegramBotClient client)
        {
            User bot = await client.GetMeAsync();
            if (bot == null)
                return null;
            Task<ChatMember> gettingChatMember = client.GetChatMemberAsync(message.Chat.Id, bot.Id);

            bool saveChangesRequired = false;

            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            Group group = await dbContext.Groups.Include(u => u.Users).FirstOrDefaultAsync(g => g.GroupId == message.Chat.Id);
            if (group == null)
            {
                group = new Group
                {
                    GroupId = message.Chat.Id,
                    Name = message.Chat.Title,
                    IsAvailable = true
                };
                await dbContext.Groups.AddAsync(group);
                try
                {
                    await dbContext.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    Console.WriteLine("Повторное добавление группы");
                    group = await dbContext.Groups.FirstOrDefaultAsync(g => g.GroupId == message.Chat.Id);
                }
            }

            if (group.Name != message.Chat.Title)
            {
                group.Name = message.Chat.Title;
                saveChangesRequired = true;
            }

            var botAsChatMember = await gettingChatMember;

            bool isChatAvailableToSend = false;
            if (botAsChatMember != null)
                isChatAvailableToSend = (botAsChatMember.CanSendMessages ?? true)
                                        && (botAsChatMember.CanSendMediaMessages ?? true)
                                        && (botAsChatMember.IsMember ?? true);

            if (group.IsAvailable != isChatAvailableToSend)
            {
                group.IsAvailable = isChatAvailableToSend;
                saveChangesRequired = true;
            }

            var senderUser = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == message.From.Id);
            if (senderUser != null && group.Users.All(u => u.UserId != senderUser.UserId))
            {
                group.Users.Add(senderUser);
                try
                {
                    await dbContext.SaveChangesAsync();
                    saveChangesRequired = false;
                }
                catch (DbUpdateException)
                {
                    Console.WriteLine("Повторное добавление юзера в группу");
                    group = await dbContext.Groups.FirstOrDefaultAsync(g => g.GroupId == message.Chat.Id);
                }
            }

            if (saveChangesRequired)
                await dbContext.SaveChangesAsync();

            return group;
        }
        protected static async Task<Group> UpdateGroupAsync(CallbackQuery callbackQuery, ITelegramBotClient client)
        {
            User bot = await client.GetMeAsync();
            if (bot == null)
                return null;
            Task<ChatMember> gettingChatMember = client.GetChatMemberAsync(callbackQuery.Message.Chat.Id, bot.Id);

            bool saveChangesRequired = false;

            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            Group group = await dbContext.Groups.Include(u => u.Users).FirstOrDefaultAsync(g => g.GroupId == callbackQuery.Message.Chat.Id);
            if (group == null)
            {
                group = new Group
                {
                    GroupId = callbackQuery.Message.Chat.Id,
                    Name = callbackQuery.Message.Chat.Title,
                    IsAvailable = true
                };
                await dbContext.Groups.AddAsync(group);
                try
                {
                    await dbContext.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    Console.WriteLine("Повторное добавление группы");
                    group = await dbContext.Groups.FirstOrDefaultAsync(g => g.GroupId == callbackQuery.Message.Chat.Id);
                }
            }

            if (group.Name != callbackQuery.Message.Chat.Title)
            {
                group.Name = callbackQuery.Message.Chat.Title;
                saveChangesRequired = true;
            }

            var botAsChatMember = await gettingChatMember;

            bool isChatAvailableToSend = false;
            if (botAsChatMember != null)
                isChatAvailableToSend = (botAsChatMember.CanSendMessages ?? true)
                                        && (botAsChatMember.CanSendMediaMessages ?? true)
                                        && (botAsChatMember.IsMember ?? true);

            if (group.IsAvailable != isChatAvailableToSend)
            {
                group.IsAvailable = isChatAvailableToSend;
                saveChangesRequired = true;
            }

            var senderUser = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == callbackQuery.From.Id);
            if (senderUser != null && group.Users.All(u => u.UserId != senderUser.UserId))
            {
                group.Users.Add(senderUser);
                try
                {
                    await dbContext.SaveChangesAsync();
                    saveChangesRequired = false;
                }
                catch (DbUpdateException)
                {
                    Console.WriteLine("Повторное добавление юзера в группу");
                }
            }

            if (saveChangesRequired)
                await dbContext.SaveChangesAsync();

            return group;
        }
        protected async Task<bool> IsUserRegisteredAsync(User user)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            return await dbContext.Users.AnyAsync(x => x.UserId == user.Id);
        }
        protected bool IsMemberRegistered(User user, Group group)
        {
            return group.Users.Any(x => x.UserId == user.Id);
        }
    }
}