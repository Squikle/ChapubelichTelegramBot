using Chapubelich.Abstractions;
using Chapubelich.ChapubelichBot.Statics;
using Chapubelich.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Chapubelich.Chating.Commands
{
    public class RegistrationCommand : Command
    {
        public override string Name => "\U0001F511 Регистрация";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(message.From.Id,
            "Пожалуйста, укажите ваш гендер:",
            replyMarkup: InlineKeyboards.genderChooseMarkup);
        }
    }
}
