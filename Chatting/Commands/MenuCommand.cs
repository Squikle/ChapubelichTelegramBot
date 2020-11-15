using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using ChapubelichBot.Types.Extensions;

namespace ChapubelichBot.Chatting.Commands
{
    class MenuCommand : Command
    {
        public override string Name => "🏠 Главная";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(
                        message.Chat.Id,
                        "\U0001F3E0 Главное меню!",
                        replyMarkup: ReplyKeyboards.MainMarkup,
                        replyToMessageId: message.MessageId);
        }
    }
}
