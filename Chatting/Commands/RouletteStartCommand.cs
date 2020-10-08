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
        public override string Name => RouletteTableStatic.Name;
        public override async void Execute(Message message, ITelegramBotClient client)
        {
            var session = RouletteTableStatic.GetGameSessionByChatId(message.Chat.Id);
            if (session == null)
            {
                RouletteTableStatic.GameSessions.Add(new RouletteGameSession(client, message));
                return;
            }

            int replyMessageId = session.GameMessage == null ? 0 : session.GameMessage.MessageId;
            await client.TrySendTextMessageAsync(message.Chat.Id,
            "Игра уже запущена!",
            replyToMessageId: replyMessageId);
        }
    }
}
