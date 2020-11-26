using System;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace ChapubelichBot.Types.Managers.MessagesSender.Limiters
{
    class ChatLimiter
    {
#pragma warning disable IDE0052
        private static object _locker = new object();
#pragma warning restore IDE0052
        private readonly Dictionary<long, RelevantChat> _relevantChats;
        public int LimitOfMessagesPerMinute { get; set; }
        public int LimitOfMessagesPerSecond { get; set; }
        public ChatLimiter(int limitOfMessagesPerMinute, int limitOfMessagesPerSecond)
        {
            _relevantChats = new Dictionary<long, RelevantChat>();
            LimitOfMessagesPerMinute = limitOfMessagesPerMinute;
            LimitOfMessagesPerSecond = limitOfMessagesPerSecond;
        }
        public bool IsAvailableToSend(ChatId chatId)
        {
            lock (_locker)
            {
                RelevantChat relevantChat = GetChat(chatId);

                return relevantChat == null || relevantChat.LastSecondMessagesSended < LimitOfMessagesPerSecond
                    && relevantChat.LastMinuteMessagesSended < LimitOfMessagesPerMinute;
            }
        }
        public void RequestCreated(ChatId chatId)
        {
            lock (_locker)
                if (!_relevantChats.TryGetValue(chatId.Identifier, out RelevantChat chat))
                    _relevantChats.Add(chatId.Identifier, new RelevantChat());
                else
                    chat.MessageSended();
        }

        private RelevantChat GetChat(ChatId chatId)
        {
            lock (_locker)
            {
                if (!_relevantChats.TryGetValue(chatId.Identifier, out RelevantChat chat))
                    return default;
                if (DateTime.Now - chat.FirstMessageSendedTime >= new TimeSpan(0, 0, 1, 1))
                {
                    _relevantChats.Remove(chatId.Identifier, out _);
                    return default;
                }
                if (DateTime.Now - chat.LastMessageSendedTime >= new TimeSpan(0, 0, 0, 1, 1))
                    chat.Reset();
                return chat;
            }
        }

        private class RelevantChat
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
            public void MessageSended()
            {
                LastMinuteMessagesSended++;
                LastSecondMessagesSended++;
                LastMessageSendedTime = DateTime.Now;
            }
            public void Reset()
            {
                LastSecondMessagesSended = 0;
            }
        }
    }
}
