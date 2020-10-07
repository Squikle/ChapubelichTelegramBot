using Telegram.Bot.Types.ReplyMarkups;

namespace Chapubelich.ChapubelichBot.Statics
{
    static class InlineKeyboards
    {
        public static InlineKeyboardMarkup genderChooseMarkup = new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("М\U00002642", "Male"),
            InlineKeyboardButton.WithCallbackData("Ж\U00002640", "Female")
        });
    }
}
