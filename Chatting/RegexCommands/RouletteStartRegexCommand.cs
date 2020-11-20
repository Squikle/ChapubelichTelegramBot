using ChapubelichBot.Types.Abstractions;
using System.Threading.Tasks;
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
            await RouletteGameManager.StartAsync(message);
        }
    }
}
