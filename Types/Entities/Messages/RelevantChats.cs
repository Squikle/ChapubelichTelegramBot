using System;
using System.Collections.Concurrent;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot.Types;

namespace ChapubelichBot.Types.Entities.Messages
{
    class RelevantChats
    {
        private readonly ConcurrentDictionary<long, RelevantChat> _relevantChats;
        public int LimitOfMessagesPerMinute { get; set; }
        public int LimitOfMessagesPerSecond { get; set; }
        public RelevantChats(int limitOfMessagesPerMinute, int limitOfMessagesPerSecond)
        {
            _relevantChats = new ConcurrentDictionary<long, RelevantChat>();
            LimitOfMessagesPerMinute = limitOfMessagesPerMinute;
            LimitOfMessagesPerSecond = limitOfMessagesPerSecond;
        }
        public void Add(ChatId chatId)
        {
            _relevantChats[chatId.Identifier] = new RelevantChat();
        }
        public RelevantChat Get(ChatId chatId)
        {
            if (!_relevantChats.ContainsKey(chatId.Identifier))
                return default;
            var chat = _relevantChats[chatId.Identifier];
            if (DateTimeOffset.Now - chat.FirstMessageSendedTime >= new TimeSpan(0, 0, 1, 0))
            {
                _relevantChats.Remove(chatId.Identifier);
                return default;
            }
            if (DateTimeOffset.Now - chat.LastMessageSendedTime >= new TimeSpan(0, 0, 0, 1))
                chat.LastSecondMessagesSended = 0;
            return _relevantChats[chatId.Identifier];
        }
        public bool Contains(ChatId chatId)
        {
            return Get(chatId) != null;
        }
        public RelevantChat this[ChatId chatId]
        {
            get => Get(chatId);
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                Add(chatId);
            }
        }
        public bool CanSendMessageToChat(ChatId chatId)
        {
            RelevantChat relevantChat = Get(chatId);
            return 
                relevantChat == null || relevantChat.LastSecondMessagesSended < LimitOfMessagesPerSecond 
                                       && relevantChat.LastMinuteMessagesSended < LimitOfMessagesPerMinute;
        }
        public void MessageSended(ChatId chatId)
        {
            RelevantChat relevantChat = Get(chatId);
            if (relevantChat == null)
                Add(chatId);
            else
            {
                relevantChat.LastMinuteMessagesSended += 1;
                relevantChat.LastSecondMessagesSended += 1;
                relevantChat.LastMessageSendedTime = DateTime.Now;
            }
        }
    }
}
