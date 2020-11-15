using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using ChapubelichBot.Types.Extensions;

namespace ChapubelichBot.Chatting.Commands
{
    public class RegistrationCommand : Command
    {
        public override string Name => "🔑 Регистрация";

        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(message.Chat.Id,
            "Пожалуйста, укажите свой пол:",
            replyMarkup: InlineKeyboards.GenderChooseMarkup);
        }
    }
}
