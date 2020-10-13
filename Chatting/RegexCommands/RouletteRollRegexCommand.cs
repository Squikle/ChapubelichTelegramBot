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
        public override string Pattern => @"^\/? *(го|ролл|погнали|крути|roll|go)(@ChapubelichBot)?$";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            var gameSession = RouletteTableStatic.GetGameSessionByChatId(message.Chat.Id);

            if (gameSession == null)
                return;

            if (!gameSession.BetTokens.Any(x => x.UserId == message.From.Id))
                await client.TrySendTextMessageAsync(
                message.Chat.Id,
                "Чтобы крутить барабан сделайте ставку",
                replyToMessageId: message.MessageId,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            else
                gameSession.ResultAsync(client, message);
        }
    }
}
