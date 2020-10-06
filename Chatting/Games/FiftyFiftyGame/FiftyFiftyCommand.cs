using Chapubelich.Abstractions;
using Chapubelich.ChapubelichBot.Init;
using Chapubelich.Chatting.Games.FiftyFiftyGame;
using Chapubelich.Extensions;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Chapubelich.Chatting.Commands
{
    class FiftyFiftyCommand : Command
    {
        public override string Name => "\U0001F3B0 50/50";
        public override async void Execute(Message message, ITelegramBotClient client)
        {
            var game = Bot.BotGamesList.First(x => x.Name == Name);
            var session = game.GameSessions.FirstOrDefault(x => x.ChatId == message.Chat.Id);
            if (null == session)
            {
                var gameSession = new FiftyFiftyGameSession(message);
                gameSession.Start(message, client);
                game.GameSessions.Add(gameSession);
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
