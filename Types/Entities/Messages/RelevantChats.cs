using System;
using System.Collections.Concurrent;
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
            _relevantChats = new ConcurrentDictionary<long, RelevantChat>(10, 10);
            LimitOfMessagesPerMinute = limitOfMessagesPerMinute;
            LimitOfMessagesPerSecond = limitOfMessagesPerSecond;
        }
        public RelevantChat GetChat(ChatId chatId)
        {
            if (!_relevantChats.TryGetValue(chatId.Identifier, out RelevantChat chat))
                return default;
            if (DateTime.Now - chat.FirstMessageSendedTime >= new TimeSpan(0, 0, 1, 1))
            {
                _relevantChats.TryRemove(chatId.Identifier, out _);
                return default;
            }

            if (DateTime.Now - chat.LastMessageSendedTime >= new TimeSpan(0, 0, 0, 1, 1))
                chat.LastSecondMessagesSended = 0;
            return chat;
        }
        public bool AvailableToSend(ChatId chatId)
        {
            RelevantChat relevantChat = GetChat(chatId);
            return relevantChat == null || relevantChat.LastSecondMessagesSended < LimitOfMessagesPerSecond
                    && relevantChat.LastMinuteMessagesSended < LimitOfMessagesPerMinute;
        }
        public void MessageSended(ChatId chatId)
        {
            _relevantChats.AddOrUpdate(chatId.Identifier, a => new RelevantChat(), (l, chat) => chat.MessageSended());
        }
    }
}
