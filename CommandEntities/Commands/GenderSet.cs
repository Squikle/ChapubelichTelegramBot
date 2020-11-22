using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.Commands
{
    class GenderSet : Command
    {
        public override string Name => "⚥  Сменить пол";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(message.From.Id,
                "Укажи свой новый пол:",
                replyMarkup: InlineKeyboards.GenderChooseMarkup,
                replyToMessageId: message.MessageId);
        }
    }
}
