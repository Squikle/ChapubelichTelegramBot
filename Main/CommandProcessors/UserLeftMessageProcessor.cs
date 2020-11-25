using System.Linq;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.CommandProcessors;
using ChapubelichBot.Types.Entities;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = ChapubelichBot.Types.Entities.User;

namespace ChapubelichBot.Main.CommandProcessors
{
    class UserLeftMessageProcessor : MessageProcessor
    {
        public override async Task<bool> ExecuteAsync(Message message, ITelegramBotClient client)
        {
            if (GlobalIgnored(message))
                return true;
            if (IsResponsiveForMessageType(message.Type) && IsResponsiveForChatType(message.Chat.Type))
            {
                return await ProcessMessageAsync(message);
            }
            return false;
        }
        protected bool IsResponsiveForMessageType(MessageType messageType)
        {
            return messageType == MessageType.ChatMemberLeft;
        }

        protected bool IsResponsiveForChatType(ChatType chatType)
        {
            return chatType == ChatType.Group || chatType == ChatType.Supergroup;
        }

        protected async Task<bool> ProcessMessageAsync(Message message)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            Group group = await dbContext.Groups.Include(g => g.Users).FirstOrDefaultAsync(g => message.Chat.Id == g.GroupId);
            User leftUser = group?.Users.FirstOrDefault(x => x.UserId == message.LeftChatMember.Id);
            if (leftUser != null && group.Users.Contains(leftUser))
            {
                group.Users.Remove(leftUser);
                await dbContext.SaveChangesAsync();
            }
            return true;
        }
    }
}
