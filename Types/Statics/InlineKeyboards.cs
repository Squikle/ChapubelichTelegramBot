﻿using Telegram.Bot.Types.ReplyMarkups;

namespace ChapubelichBot.Types.Statics
{
    static class InlineKeyboards
    {
        public static InlineKeyboardMarkup GenderChooseMarkup => new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("М\U00002642", "Male"),
            InlineKeyboardButton.WithCallbackData("Ж\U00002640", "Female")
        });

        public static InlineKeyboardMarkup ComplimentSubscriptionChooseMarkup => new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("Выключить\U0001F494", "DisableCompliments"),
            InlineKeyboardButton.WithCallbackData("Включить\U0001F49A", "EnableCompliments")
        });

        public static InlineKeyboardMarkup DefaultBetChooseMarkup => new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("25", "DefaultBet25"), },
            new[] { InlineKeyboardButton.WithCallbackData("50", "DefaultBet50"), },
            new[] { InlineKeyboardButton.WithCallbackData("100", "DefaultBet100") },
            new[] { InlineKeyboardButton.WithCallbackData("500", "DefaultBet500") }
        });

        public static InlineKeyboardMarkup RoulettePlayAgainMarkup => new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("Играть снова\U0001F501", "roulettePlayAgain")
        });

        public static InlineKeyboardMarkup AliasPlayAgainMarkup => new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("Играть снова\U0001F501", "aliasPlayAgain")
        });

        public static InlineKeyboardMarkup RouletteBetsMarkup => new InlineKeyboardMarkup(new[]
        {
            new[]
                    {
                        InlineKeyboardButton.WithCallbackData("\U0001F534Красный (1:1)", "rouletteBetRed"),
                        InlineKeyboardButton.WithCallbackData("\U000026ABЧерный (1:1)", "rouletteBetBlack"),
                        InlineKeyboardButton.WithCallbackData("\U0001F7E2Зеленый (35:1)", "rouletteBetGreen")
                    },
            new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Четные (1:1)", "rouletteBetEven"),
                        InlineKeyboardButton.WithCallbackData("Нечетные (1:1)", "rouletteBetOdd")
                    },
            new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Малые (1-18) (1:1)", "rouletteBetFirstHalf"),
                        InlineKeyboardButton.WithCallbackData("Большие (19-36) (1:1)", "rouletteBetSecondHalf")
                    },
            new[]
                    {
                        InlineKeyboardButton.WithCallbackData("1st 12 (1-12) (2:1)", "rouletteBetFirstTwelve"),
                        InlineKeyboardButton.WithCallbackData("2nd 12 (13-24) (2:1)", "rouletteBetSecondTwelve"),
                        InlineKeyboardButton.WithCallbackData("3rd 12 (25-36) (2:1)", "rouletteBetThirdTwelve")
                    },
            new[]
                    {
                        InlineKeyboardButton.WithCallbackData("1 2to1 (2:1)", "rouletteBetFirstRow"),
                        InlineKeyboardButton.WithCallbackData("2 2to1 (2:1)", "rouletteBetSecondRow"),
                        InlineKeyboardButton.WithCallbackData("3 2to1 (2:1)", "rouletteBetThirdRow")
                    },
            new[]
                    {
                        InlineKeyboardButton.WithCallbackData("✅Крутить✅", "rouletteRoll"),
                    },
            new[]
                    {
                        InlineKeyboardButton.WithCallbackData("❌Отмена ставок❌", "rouletteBetsCancel"),
                    }
        });

        public static InlineKeyboardMarkup AliasRegistrationMarkup => new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("\U0001F451Быть ведущим\U0001F451", "hostAliasRequest")
        });

        public static InlineKeyboardMarkup GetAliasChooseWordMarkup(string word1, string word2, string word3)
        {
            return new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{word1}", "aliasChooseFirstWord")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{word2}", "aliasChooseSecondWord")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{word3}", "aliasChooseThirdWord")
                }
            };
        }
    }
}
