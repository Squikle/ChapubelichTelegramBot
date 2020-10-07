using Chapubelich.Abstractions;
using Chapubelich.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Game = Chapubelich.Abstractions.Game;

namespace Chapubelich.Chatting.Games.FiftyFiftyGame
{
    class FiftyFiftyRollRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *(го|ролл|погнали|крути|roll)(@ChapubelichBot)?$";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            var gameSession = FiftyFiftyGame.GetGameSessionByChatId(message.Chat.Id);


            if (null != gameSession)
            {
                if (!gameSession.BetTokens.Any())
                    await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Ждем ставки и начинаем",
                    replyToMessageId: message.MessageId,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                else
                    gameSession.Result(client, message: message);
            }

            
        }
    }
}
