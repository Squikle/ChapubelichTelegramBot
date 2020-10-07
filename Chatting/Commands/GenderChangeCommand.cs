using Chapubelich.Abstractions;
using Chapubelich.ChapubelichBot.Statics;
using Chapubelich.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Chapubelich.Chatting.Commands
{
    class GenderChangeCommand : Command
    {
        public override string Name => "\U000026A5 Сменить пол";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(message.From.Id,
                "Пожалуйста, укажите ваш гендер:",
                replyMarkup: InlineKeyboards.genderChooseMarkup,
                replyToMessageId: message.MessageId);
        }
    }
}
