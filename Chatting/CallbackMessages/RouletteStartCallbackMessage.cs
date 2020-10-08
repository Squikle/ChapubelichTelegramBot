using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using ChapubelichBot.Types.Extensions;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Types;
using ChapubelichBot.Types.Games.RouletteGame;

namespace ChapubelichBot.Chatting.CallbackMessages
{
    class RouletteStartCallbackMessage : CallBackMessage
    {
        public override List<string> IncludingData => new List<string> { "roulettePlayAgain" };

        public override async void Execute(CallbackQuery query, ITelegramBotClient client)
        {
            var session = RouletteTableStatic.GetGameSessionByChatId(query.Message.Chat.Id);
            if (null == session)
            {
                await client.TryEditMessageReplyMarkupAsync(query.Message.Chat.Id, query.Message.MessageId);
                RouletteTableStatic.GameSessions.Add(new RouletteGameSession(client, query.Message));
                return;
            }

            if (session.GameMessage != null)
                await client.TrySendTextMessageAsync(query.Message.Chat.Id,
                "Игра уже запущена!",
                replyToMessageId: session.GameMessage.MessageId);
        }
    }
} 
