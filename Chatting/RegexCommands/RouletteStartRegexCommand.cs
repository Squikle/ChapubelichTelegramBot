using ChapubelichBot.Types.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using ChapubelichBot.Types.Games.RouletteGame;
<<<<<<< HEAD
using ChapubelichBot.Types.Statics;
=======
using ChapubelichBot.Types.Extensions;
>>>>>>> f51eb8cf7a13bedff784b9c601016a242bde30df

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
