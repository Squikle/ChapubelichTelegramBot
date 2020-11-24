using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Managers;
using ChapubelichBot.Types.Managers.MessagesSender;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.Commands
{
    class DefaultBetSet : Command
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
