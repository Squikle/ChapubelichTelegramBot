using Chapubelich.Abstractions;
using Chapubelich.ChapubelichBot.Statics;
using Chapubelich.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Chapubelich.Chatting.Commands
{
    class GamesCommand : Command
    {
        public override string Name => "\U0001F579 Игры";

        public override async void Execute(Message message, ITelegramBotClient client)
        {          
            await client.TrySendTextMessageAsync(
                        message.Chat.Id,
                        "\U0001F579 Список доступных игр!",
                        replyMarkup: ReplyKeyboards.GameMarkup,
                        replyToMessageId: message.MessageId);
        }
    }
}
