using System;
using System.Collections.Concurrent;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot.Types;

namespace ChapubelichBot.Types.Entities.Messages
{
    class RelevantChats
    {
        private readonly ConcurrentDictionary<long, RelevantChat> _relevantChats;
        public RelevantChats()
        {
            _relevantChats = new ConcurrentDictionary<long, RelevantChat>();
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
            if (DateTimeOffset.Now - chat.LastMessageSended >= new TimeSpan(0,0,0,1))
            {
                _relevantChats.Remove(chatId.Identifier);
                return default;
            }
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
            int limitOfMessagesPerSecond = 1;
            RelevantChat relevantChat = Get(chatId);
            return relevantChat == null || relevantChat.MessagesSended < limitOfMessagesPerSecond - 1;
        }
    }
}
