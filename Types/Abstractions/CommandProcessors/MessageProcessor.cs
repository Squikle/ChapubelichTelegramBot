using System.Linq;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Entities;
using ChapubelichBot.Types.Managers;
using ChapubelichBot.Types.Managers.MessagesSender;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChapubelichBot.Types.Abstractions.CommandProcessors
{
    public abstract class MessageProcessor
    {
        public abstract Task<bool> ExecuteAsync(Message message, ITelegramBotClient client);

        protected static async Task SendRegistrationAlertAsync(Message message, ITelegramBotClient client)
        {
            if (message.Chat.Type == ChatType.Private)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Упс, кажется тебя нет в базе данных. Пожалуйста, пройди процесс регистрации: ",
                    replyToMessageId: message.MessageId);
                await ChapubelichClient.RegistrationCommand.ExecuteAsync(message, client);
            }
            else if (message.Chat.Type == ChatType.Group ||
                     message.Chat.Type == ChatType.Supergroup)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Кажется, вы не зарегистрированы. Для регистрации обратитесь к боту в личные сообщения 💌",
                    replyToMessageId: message.MessageId
                );
            }
        }
        protected static async Task<Group> UpdateGroupAsync(Message message, ITelegramBotClient client)
        {
            Telegram.Bot.Types.User bot = await client.GetMeAsync();
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
                saveChangesRequired = true;
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
                saveChangesRequired = true;
            }

            if (saveChangesRequired)
                await dbContext.SaveChangesAsync();

            return group;
        }
        protected async Task<bool> IsUserRegisteredAsync(Telegram.Bot.Types.User user)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            return await dbContext.Users.AnyAsync(x => x.UserId == user.Id);
        }
        protected bool IsMemberRegistered(Telegram.Bot.Types.User user, Group group)
        {
            return group.Users.Any(x => x.UserId == user.Id);
        }
        protected bool GlobalIgnored(Message message)
        {
            return message.ForwardFrom != null;
        }
    }
}
