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

        public static InlineKeyboardMarkup defaultBetChooseMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[][]
        {
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("25", "DefaultBet25"), },
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("50", "DefaultBet50"), },
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("100", "DefaultBet100") },
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("500", "DefaultBet500") }
        });

        public static InlineKeyboardMarkup roulettePlayAgainMarkup = new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("Играть снова\U0001F501", "roulettePlayAgain")
        });

        public static InlineKeyboardMarkup rouletteBetsMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[][]
        {
            new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("\U0001F534Красный (1:1)", "rouletteBetRed"),
                        InlineKeyboardButton.WithCallbackData("\U000026ABЧерный (1:1)", "rouletteBetBlack"),
                        InlineKeyboardButton.WithCallbackData("\U0001F7E2Зеленый (35:1)", "rouletteBetGreen")
                    },
            new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("Четные (1:1)", "rouletteBetEven"),
                        InlineKeyboardButton.WithCallbackData("Нечетные (1:1)", "rouletteBetOdd")
                    },
            new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("Малые (1-18) (1:1)", "rouletteBetFirstHalf"),
                        InlineKeyboardButton.WithCallbackData("Большие (19-36) (1:1)", "rouletteBetSecondHalf")
                    },
            new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("1st 12 (1-12) (2:1)", "rouletteBetFirstTwelve"),
                        InlineKeyboardButton.WithCallbackData("2nd 12 (13-24) (2:1)", "rouletteBetSecondTwelve"),
                        InlineKeyboardButton.WithCallbackData("3rd 12 (25-36) (2:1)", "rouletteBetThirdTwelve")
                    },
            new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("1 2to1 (2:1)", "rouletteBetFirstRow"),
                        InlineKeyboardButton.WithCallbackData("2 2to1 (2:1)", "rouletteBetSecondRow"),
                        InlineKeyboardButton.WithCallbackData("3 2to1 (2:1)", "rouletteBetThirdRow")
                    },
            new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("✅Крутить✅", "rouletteRoll"),
                    },
            new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("❌Отмена ставок❌", "rouletteBetsCancel"),
                    }
        });
    }
}
