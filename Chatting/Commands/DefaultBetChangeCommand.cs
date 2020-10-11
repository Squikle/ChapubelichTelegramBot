using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.Commands
{
    class DefaultBetChangeCommand : Command
    {
        public override string Name => "\U0001F4B8 Ставка по умолчанию";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(message.From.Id,
            "Пожалуйста, выберите ставку по умолчанию:",
            replyMarkup: InlineKeyboardsStatic.defaultBetChooseMarkup,
            replyToMessageId: message.MessageId);
        }
    }
}
