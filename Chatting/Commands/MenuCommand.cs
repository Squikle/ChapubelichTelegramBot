using Chapubelich.Abstractions;
using Chapubelich.ChapubelichBot.Statics;
using Chapubelich.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chapubelich.Chatting.Commands
{
    class MenuCommand : Command
    {
        public override string Name => "\U0001F3E0 Главная";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(
                        message.Chat.Id,
                        "\U0001F3E0 Главное меню!",
                        replyMarkup: ReplyKeyboards.MainMarkup,
                        replyToMessageId: message.MessageId);
        }
    }
}
