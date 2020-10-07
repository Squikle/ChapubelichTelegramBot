using Chapubelich.Abstractions;
using Chapubelich.ChapubelichBot.Init;
using Chapubelich.Chatting.Games.FiftyFiftyGame;
using Chapubelich.Extensions;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Chapubelich.Chatting.Commands
{
    class FiftyFiftyStartCommand : Command
    {
        public override string Name => FiftyFiftyGame.Name;
        public override async void Execute(Message message, ITelegramBotClient client)
        {
            var session = FiftyFiftyGame.GetGameSessionByChatId(message.Chat.Id);
            if (null == session)
            {
                var gameSession = new FiftyFiftyGameSession(message);
                gameSession.Start(message, client);
                FiftyFiftyGame.GameSessions.Add(gameSession);
            }
            else
            {
                if (session.GameMessage != null)
                    await client.TrySendTextMessageAsync(message.Chat.Id,
                    "Игра уже запущена!",
                    replyToMessageId: session.GameMessage.MessageId);
            }
        }
    }
}
