using Telegram.Bot.Types.ReplyMarkups;

namespace ChapubelichBot.Types.Statics
{
    static class InlineKeyboardsStatic
    {
        public static InlineKeyboardMarkup genderChooseMarkup = new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("М\U00002642", "Male"),
            InlineKeyboardButton.WithCallbackData("Ж\U00002640", "Female")
        });

        public static InlineKeyboardMarkup roulettePlayAgainMarkup = new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("Играть снова\U0001F501", "roulettePlayAgain")
        });
    }
}
