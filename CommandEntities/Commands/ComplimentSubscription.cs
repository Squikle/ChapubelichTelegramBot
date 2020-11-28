using System;
using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Managers.MessagesSender;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.Commands
{
    class ComplimentSubscription : Command
    {
        public override string Name => "🙌 Комплимент дня";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(message.From.Id,
                "Выбери опцию подписки: ",
                replyMarkup: InlineKeyboards.ComplimentSubscriptionChooseMarkup,
                replyToMessageId: message.MessageId);
        }
    }
}
