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
using User = ChapubelichBot.Types.Entities.User;

namespace ChapubelichBot.Types.Abstractions.CommandProcessors
{
    public abstract class CallbackMessageProcessor
    {
        public abstract Task<bool> ExecuteAsync(CallbackQuery callbackQuery, ITelegramBotClient client);
        protected abstract bool IsResponsiveForChatType(ChatType chatType);
        protected abstract Task<bool> ProcessCallBackMessageAsync(CallbackQuery callBackQuery, bool isUserRegistered, ITelegramBotClient client);
        protected static async Task SendRegistrationAlertAsync(CallbackQuery callbackQuery, ITelegramBotClient client)
        {
            await client.TryAnswerCallbackQueryAsync(
                callbackQuery.Id,
                "Пожалуйста, пройдите процесс регистрации.",
                showAlert: true);
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

            if (message.Type == MessageType.ChatMemberLeft)
            {
                User leftUser = group.Users.FirstOrDefault(x => x.UserId == message.LeftChatMember.Id);
                if (leftUser != null && group.Users.Contains(leftUser))
                {
                    group.Users.Remove(leftUser);
                    saveChangesRequired = true;
                }
            }
            else
            {
                var senderUser = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == message.From.Id);
                if (senderUser != null && group.Users.All(u => u.UserId != senderUser.UserId))
                {
                    group.Users.Add(senderUser);
                    saveChangesRequired = true;
                }
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
        protected bool GlobalIgnored(CallbackQuery callbackQuery)
        {
            return callbackQuery.Data == null;
        }
    }
}
