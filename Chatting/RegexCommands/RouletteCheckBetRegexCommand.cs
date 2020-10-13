using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using ChapubelichBot.Database;
using ChapubelichBot.Types.Extensions;
using System.Threading.Tasks;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = ChapubelichBot.Database.Models.User;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteCheckBetRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *((мо(и|я))|(my))?\s*((ставк(и|а))|(bets?))(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            var gameSession = RouletteTableStatic.GetGameSessionOrNull(message.Chat.Id);
            if (gameSession != null)
                await gameSession.CheckBet(message, client);
        }
    }
}