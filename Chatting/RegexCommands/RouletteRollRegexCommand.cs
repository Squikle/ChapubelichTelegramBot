using ChapubelichBot.Types.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using ChapubelichBot.Types.Games.RouletteGame;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteRollRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *(го|ролл|погнали|крути|roll|go)(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            var gameSession = RouletteGameManager.GetGameSessionOrNull(message.Chat.Id);
            if (gameSession != null)
                await RouletteGameManager.RollRequest(message);
        }
    }
}
