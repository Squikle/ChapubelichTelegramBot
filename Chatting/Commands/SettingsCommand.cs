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
    class SettingsCommand : Command
    {
        public override string Name => "\U00002699 Настройки";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(
            message.Chat.Id,
            "\U00002699 Меню настроек!",
            replyMarkup: ReplyKeyboards.SettingsMarkup, 
            replyToMessageId: message.MessageId);
        }
    }
}
