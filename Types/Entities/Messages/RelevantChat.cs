using System;

namespace ChapubelichBot.Types.Entities.Messages
{
    class RelevantChat
    {
        public int LastSecondMessagesSended { get; set; }
        public int LastMinuteMessagesSended { get; set; }
        public DateTime FirstMessageSendedTime { get; set; }
        public DateTime LastMessageSendedTime { get; set; }

        public RelevantChat()
        {
            LastMinuteMessagesSended = 1;
            LastSecondMessagesSended = 1;
            FirstMessageSendedTime = DateTime.Now;
            LastMessageSendedTime = DateTime.Now;
        }
    }
}
