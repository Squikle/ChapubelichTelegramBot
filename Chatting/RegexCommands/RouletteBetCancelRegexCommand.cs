using ChapubelichBot.Types.Abstractions;
using System.Threading.Tasks;
using ChapubelichBot.Types.Games.RouletteGame;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteBetCancelRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *(отмена|cancel)(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            var gameSession = RouletteGameManager.GetGameSessionOrNull(message.Chat.Id);
            if (gameSession != null)
                await RouletteGameManager.BetCancelRequest(message);
        }
    }
}
