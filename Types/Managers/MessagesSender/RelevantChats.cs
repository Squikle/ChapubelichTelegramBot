using System;
using System.Collections.Generic;
using ChapubelichBot.Types.Managers.MessagesSender;
using Telegram.Bot.Types;

namespace ChapubelichBot.Types.Entities.Messages
{
    class RelevantChats
    {


        private readonly Dictionary<long, RelevantChat> _relevantChats;
        public int LimitOfMessagesPerMinute { get; set; }
        public int LimitOfMessagesPerSecond { get; set; }
        public RelevantChats(int limitOfMessagesPerMinute, int limitOfMessagesPerSecond)
        {
            _relevantChats = new Dictionary<long, RelevantChat>();
            LimitOfMessagesPerMinute = limitOfMessagesPerMinute;
            LimitOfMessagesPerSecond = limitOfMessagesPerSecond;
        }
        public bool AvailableToSend(ChatId chatId)
        {
            RelevantChat relevantChat = GetChat(chatId);

            return relevantChat == null || relevantChat.LastSecondMessagesSended < LimitOfMessagesPerSecond
                && relevantChat.LastMinuteMessagesSended < LimitOfMessagesPerMinute;
        }
        public void MessageSended(ChatId chatId)
        {
            if (!_relevantChats.TryGetValue(chatId.Identifier, out RelevantChat chat))
                _relevantChats.Add(chatId.Identifier, new RelevantChat());
            else
                chat.MessageSended();
        }

        private RelevantChat GetChat(ChatId chatId)
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
}
