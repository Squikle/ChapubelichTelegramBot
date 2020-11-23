using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Managers;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.Commands
{
    class SettingsList : Command
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
