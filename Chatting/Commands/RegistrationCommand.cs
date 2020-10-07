using Chapubelich.Abstractions;
using Chapubelich.ChapubelichBot.Statics;
using Chapubelich.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chapubelich.Chating.Commands
{
    public class RegistrationCommand : Command
    {
        public override string Name => "\U0001F511 Register";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            InlineKeyboardMarkup genderChooseMarkup = new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData("М\U00002642", "Male"),
                InlineKeyboardButton.WithCallbackData("Ж\U00002640", "Female")
            });

            await client.TrySendTextMessageAsync(message.From.Id,
            "Пожалуйста, укажите ваш гендер:",
            replyMarkup: genderChooseMarkup);
        }
    }
}
