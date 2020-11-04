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
            var gameMessage = await RouletteGameSession.InitializeNew(message, client);

            if (gameMessage != null)
                await client.TrySendTextMessageAsync(message.Chat.Id,
                "Игра уже запущена!",
                replyToMessageId: gameMessage.MessageId);
        }
    }
}
