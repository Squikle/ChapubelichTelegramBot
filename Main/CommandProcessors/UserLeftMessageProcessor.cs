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
        public override async Task<bool> Execute(Message message, ITelegramBotClient client)
        {
            if (GlobalIgnored(message))
                return true;
            if (IsResponsiveForMessageType(message.Type) && IsResponsiveForChatType(message.Chat.Type))
            {
                return await ProcessMessage(message, client);
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

        protected async Task<bool> ProcessMessage(Message message, ITelegramBotClient client)
        {
            await using var db = new ChapubelichdbContext();
            Group group = db.Groups.Include(g => g.Users).FirstOrDefault(g => message.Chat.Id == g.GroupId);
            User leftUser = group?.Users.FirstOrDefault(x => x.UserId == message.LeftChatMember.Id);
            if (leftUser != null && group.Users.Contains(leftUser))
            {
                group.Users.Remove(leftUser);
                db.SaveChanges();
            }
            return true;
        }
    }
}
