using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteRollRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *(го|ролл|погнали|крути|roll|go)(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            var gameSession = RouletteTableStatic.GetGameSessionOrNull(message.Chat.Id);
            if (gameSession != null)
                await gameSession.RollRequest(message, client);
        }
    }
}
