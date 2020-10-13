using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;

namespace ChapubelichBot.Chatting.Commands
{
    class MenuCommand : Command
    {
        public override string Name => "\U0001F3E0 Главная";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(
                        message.Chat.Id,
                        "\U0001F3E0 Главное меню!",
                        replyMarkup: ReplyKeyboardsStatic.MainMarkup,
                        replyToMessageId: message.MessageId);
        }
    }
}
