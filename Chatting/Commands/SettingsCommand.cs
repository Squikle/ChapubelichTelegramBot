using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;

namespace ChapubelichBot.Chatting.Commands
{
    class SettingsCommand : Command
    {
        public override string Name => "⚙️ Настройки";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(
            message.Chat.Id,
            "\U00002699 Меню настроек!",
            replyMarkup: ReplyKeyboards.SettingsMarkup, 
            replyToMessageId: message.MessageId);
        }
    }
}
