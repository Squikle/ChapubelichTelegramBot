using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using ChapubelichBot.Types.Extensions;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteRollRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *(го|ролл|погнали|крути|roll)(@ChapubelichBot)?$";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            var gameSession = RouletteGameStatic.GetGameSessionByChatId(message.Chat.Id);


            if (null != gameSession)
            {
                if (!gameSession.BetTokens.Any())
                    await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Ждем ставки и начинаем",
                    replyToMessageId: message.MessageId,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                else
                    gameSession.Result(client, message);
            }

            
        }
    }
}
