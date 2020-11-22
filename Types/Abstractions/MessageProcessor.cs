using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChapubelichBot.Types.Abstractions
{
    abstract class MessageProcessor
    {
        public abstract bool IsResponsiveForMessageType(MessageType messageType);
        public abstract bool IsResponsiveForChatType(ChatType chatType);
        public abstract Task<bool> ProcessMessage(Message message);
    }
}
