using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Games.RouletteGame;
using System.Threading.Tasks;
using ChapubelichBot.Database.Models;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.Commands
{
    class RouletteStartCommand : Command
    {
        public override string Name => RouletteGameManager.Name;
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            RouletteGameSession gameSession = RouletteGameManager.GetGameSessionOrNull(message.Chat.Id);
            if (gameSession != null)
            {
                await client.TrySendTextMessageAsync(message.Chat.Id,
                    "Игра уже запущена!",
                    replyToMessageId: gameSession.GameMessageId);
            }
            else
            { 
                await RouletteGameManager.StartAsync(message);
            }
        }
    }
}
