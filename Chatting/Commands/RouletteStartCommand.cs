using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using ChapubelichBot.Types.Games.RouletteGame;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.Commands
{
    class RouletteStartCommand : Command
    {
        public override string Name => RouletteTableStatic.Name;
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            RouletteGameSession gameSession = RouletteTableStatic.GetGameSessionOrNull(message.Chat.Id);
            if (gameSession == null)
                await RouletteTableStatic.InitializeNew(message, client);
            else
            {
                await client.TrySendTextMessageAsync(message.Chat.Id,
                    "Игра уже запущена!",
                    replyToMessageId: gameSession.GameMessage.MessageId);
            }
        }
    }
}
