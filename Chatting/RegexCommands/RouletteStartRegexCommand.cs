using ChapubelichBot.Types.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using ChapubelichBot.Types.Games.RouletteGame;
using ChapubelichBot.Types.Extensions;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteStartRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *(рулетка|roulette)(@ChapubelichBot)?$";

        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            var gameMessage = await RouletteGameSession.InitializeNew(message, client);

            if (gameMessage != null)
            {
                await client.TrySendTextMessageAsync(message.Chat.Id,
                "Игра уже запущена!",
                replyToMessageId: gameMessage.MessageId);
            }
        }
    }
}
