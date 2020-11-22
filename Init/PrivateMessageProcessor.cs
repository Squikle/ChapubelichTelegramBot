using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChapubelichBot.Init
{
    class PrivateMessageProcessor : MessageProcessor
    {
        public override bool IsResponsiveForMessageType(MessageType messageType)
        {
            return messageType == MessageType.Text;
        }
        public override bool IsResponsiveForChatType(ChatType chatType)
        {
            return chatType == ChatType.Private;
        }

        public override Task<bool> ProcessMessage(Message message)
        {
            throw new NotImplementedException();
        }
    }
}
