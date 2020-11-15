using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteBetInfoRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *((мо(и|я))|(my))?\s*((ставк(и|а))|(bets?))(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            var gameSession = RouletteTableStatic.GetGameSessionOrNull(message.Chat.Id);
            if (gameSession != null)
                await gameSession.BetInfoRequest(message, client);
        }
    }
}