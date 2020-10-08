using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using ChapubelichBot.Types.Games.RouletteGame;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.Commands
{
    class FiftyFiftyStartCommand : Command
    {
        public override string Name => RouletteGameStatic.Name;
        public override async void Execute(Message message, ITelegramBotClient client)
        {
            var session = RouletteGameStatic.GetGameSessionByChatId(message.Chat.Id);
            if (session == null)
            {
                RouletteGameStatic.GameSessions.Add(new RouletteGameSession(client, message));
                return;
            }
                await client.TrySendTextMessageAsync(message.Chat.Id,
                "Игра уже запущена!",
                replyToMessageId: session.GameMessage.MessageId);
        }
    }
}
