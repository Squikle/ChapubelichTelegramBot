using ChapubelichBot.Types.Abstractions;
using System.Threading.Tasks;
using ChapubelichBot.Database.Models;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using ChapubelichBot.Types.Games.RouletteGame;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteStartRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *(рулетка|roulette)(@ChapubelichBot)?$";

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
