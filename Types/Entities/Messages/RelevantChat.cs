using System;
using System.Threading;

namespace ChapubelichBot.Types.Entities.Messages
{
    class RelevantChat
    {
        public int LastSecondMessagesSended { get; set; }
        public int LastMinuteMessagesSended { get; set; }
        public DateTime FirstMessageSendedTime { get; set; }
        public DateTime LastMessageSendedTime { get; set; }
        private Mutex _mutex = new Mutex();

        public RelevantChat()
        {
            LastMinuteMessagesSended = 1;
            LastSecondMessagesSended = 1;
            FirstMessageSendedTime = DateTime.Now;
            LastMessageSendedTime = DateTime.Now;
        }

        public RelevantChat MessageSended()
        {
            _mutex.WaitOne();
            LastMinuteMessagesSended++;
            LastSecondMessagesSended++;
            LastMessageSendedTime = DateTime.Now;
            _mutex.ReleaseMutex();

            return this;
        }

        public void Reset()
        {
            _mutex.WaitOne();
            LastSecondMessagesSended = 0;
            _mutex.ReleaseMutex();
        }
    }
}
