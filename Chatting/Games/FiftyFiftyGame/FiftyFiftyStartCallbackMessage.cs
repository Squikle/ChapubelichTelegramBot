using Chapubelich.Abstractions;
using Chapubelich.ChapubelichBot.Init;
using Chapubelich.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Chapubelich.Chatting.Games.FiftyFiftyGame
{
    class FiftyFiftyStartCallbackMessage : CallBackMessage
    {
        public override List<string> IncludingData => new List<string> { "50/50 PlayAgain" };

        public override async void Execute(CallbackQuery query, ITelegramBotClient client)
        {
            var session = FiftyFiftyGame.GetGameSessionByChatId(query.Message.Chat.Id);
            if (null == session)
            {
                await client.TryEditMessageReplyMarkupAsync(query.Message.Chat.Id, query.Message.MessageId);
                var gameSession = new FiftyFiftyGameSession(query.Message);
                gameSession.Start(query.Message, client);
                FiftyFiftyGame.GameSessions.Add(gameSession);
            }
            else
            {
                if (session.GameMessage != null)
                    await client.TrySendTextMessageAsync(query.Message.Chat.Id,
                    "Игра уже запущена!",
                    replyToMessageId: session.GameMessage.MessageId);
            }
        }
    }
} 
