using System;

namespace ChapubelichBot.Types.Entities.Messages
{
    class RelevantChat
    {
        public int MessagesSended { get; set; }
        public DateTime LastMessageSended { get; set; }

        public RelevantChat()
        {
            MessagesSended = 0;
            LastMessageSended = DateTime.Now;
        }
    }
}
