using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.Commands
{
    class SettingsCommand : Command
    {
        public override string Name => "\U00002699 Настройки";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(
            message.Chat.Id,
            "\U00002699 Меню настроек!",
            replyMarkup: ReplyKeyboardsStatic.SettingsMarkup, 
            replyToMessageId: message.MessageId);
        }
    }
}
