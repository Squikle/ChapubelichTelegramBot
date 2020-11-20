using ChapubelichBot.Types.Abstractions;
using System.Threading.Tasks;
using ChapubelichBot.Database;
using ChapubelichBot.Database.Models;
using ChapubelichBot.Types.Games.RouletteGame;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteBetInfoRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *((мо(и|я))|(my))?\s*((ставк(и|а))|(bets?))(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            RouletteGameSession gameSession = null;
            await using (var db = new ChapubelichdbContext())
            {
                gameSession = RouletteGameManager.GetGameSessionOrNull(message.Chat.Id, db);
            }
            if (gameSession != null)
                await RouletteGameManager.BetInfoRequest(message);
        }
    }
}