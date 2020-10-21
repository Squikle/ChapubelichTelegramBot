using ChapubelichBot.Init;
using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using System.Threading.Tasks;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using ChapubelichBot.Types.Games.RouletteGame;
using ChapubelichBot.Types.Extensions;
using System;

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
                int replyId = gameMessage == null ? 0 : gameMessage.MessageId;
                await client.TrySendTextMessageAsync(message.Chat.Id,
                "Игра уже запущена!",
                replyToMessageId: gameMessage.MessageId);
            }
        }
    }
}
