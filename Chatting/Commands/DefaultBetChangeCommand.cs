using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using System.Threading.Tasks;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.Commands
{
    class DefaultBetChangeCommand : Command
    {
        public override string Name => "💸 Ставка по умолчанию";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(message.From.Id,
            "Выбери новую ставку по умолчанию:",
            replyMarkup: InlineKeyboards.DefaultBetChooseMarkup,
            replyToMessageId: message.MessageId);
        }
    }
}
