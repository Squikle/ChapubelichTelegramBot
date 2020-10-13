using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using ChapubelichBot.Types.Extensions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.Commands
{
    class GenderChangeCommand : Command
    {
        public override string Name => "\U000026A5  Сменить пол";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(message.From.Id,
                "Пожалуйста, укажите ваш гендер:",
                replyMarkup: InlineKeyboardsStatic.genderChooseMarkup,
                replyToMessageId: message.MessageId);
        }
    }
}
