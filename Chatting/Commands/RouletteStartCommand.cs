using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using ChapubelichBot.Types.Games.RouletteGame;
using ChapubelichBot.Types.Extensions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.Commands
{
    class FiftyFiftyStartCommand : Command
    {
        public override string Name => RouletteTableStatic.Name;
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            var session = RouletteTableStatic.GetGameSessionByChatId(message.Chat.Id);
            if (session == null)
            {
                RouletteTableStatic.GameSessions.Add(await RouletteGameSession.Initialize(client, message));
                return;
            }

            int replyMessageId = session.GameMessage == null ? 0 : session.GameMessage.MessageId;
            await client.TrySendTextMessageAsync(message.Chat.Id,
            "Игра уже запущена!",
            replyToMessageId: replyMessageId);
        }
    }
}
