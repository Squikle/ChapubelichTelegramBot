using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.Commands
{
    class GenderChangeCommand : Command
    {
        public override string Name => "⚥  Сменить пол";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(message.From.Id,
                "Укажи свой новый пол:",
                replyMarkup: InlineKeyboardsStatic.GenderChooseMarkup,
                replyToMessageId: message.MessageId);
        }
    }
}
