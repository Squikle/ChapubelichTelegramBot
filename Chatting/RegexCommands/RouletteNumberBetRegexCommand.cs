using ChapubelichBot.Types.Abstractions;
using System.Threading.Tasks;
using ChapubelichBot.Types.Games.RouletteGame;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteNumberBetRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *(\d+) +([0-9]|[1-3][0-9])( *- *([0-9]|[1-3][0-9]))? *(го|ролл|погнали|крути|roll|go)?(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            var gameSession = RouletteGameManager.GetGameSessionOrNull(message.Chat.Id);
            if (gameSession != null)
                await RouletteGameManager.BetNumbersRequest(message, Pattern);
        }
    }
}
