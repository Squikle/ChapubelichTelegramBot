using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using ChapubelichBot.Types.Games.RouletteGame;
using System.Threading.Tasks;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.Commands
{
    class RouletteStartCommand : Command
    {
        public override string Name => RouletteGame.Name;
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            RouletteGameSession gameSession = RouletteGame.GetGameSessionOrNull(message.Chat.Id);
            if (gameSession == null)
                await RouletteGameSessionBuilder.Create().InitializeNew(message, client).AddToSessionsList().Build()
                    .Start(message, client);
            else
            {
                await client.TrySendTextMessageAsync(message.Chat.Id,
                    "Игра уже запущена!",
                    replyToMessageId: gameSession.GameMessageId);
            }
        }
    }
}
