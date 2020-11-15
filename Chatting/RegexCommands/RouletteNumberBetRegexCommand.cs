using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteNumberBetRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *(\d{1,4}) +([0-9]|[1-3][0-9])( *- *([0-9]|[1-3][0-9]))? *(го|ролл|погнали|крути|roll|go)?(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            var gameSession = RouletteGame.GetGameSessionOrNull(message.Chat.Id);

            if (gameSession != null)
                await gameSession.BetNumbersRequest(message, Pattern, client);
        }
    }
}
