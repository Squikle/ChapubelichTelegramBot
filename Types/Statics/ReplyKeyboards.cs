using ChapubelichBot.Types.Managers;
using Telegram.Bot.Types.ReplyMarkups;

namespace ChapubelichBot.Types.Statics
{
    static class ReplyKeyboards
    {
        public static ReplyKeyboardMarkup MainMarkup = new ReplyKeyboardMarkup(
            new[]
            {
                    new[] 
                    { 
                        new KeyboardButton("💰 Баланс"),
                        new KeyboardButton("🕹 Игры")
                    },
                    new[]
                    {
                        new KeyboardButton("⚙️ Настройки")
                    },
                    new[]
                    {
                        new KeyboardButton("💵 Ежедневная награда")
                    },
                    new[] 
                    { 
                        new KeyboardButton("❓ Помощь") 
                    }
                }, resizeKeyboard: true, oneTimeKeyboard: false);

        public static ReplyKeyboardMarkup GameMarkup = new ReplyKeyboardMarkup(
            new[]
            {
                    new[]
                    {
                        new KeyboardButton(RouletteGameManager.Name)
                    },
                    new[]
                    {
                        new KeyboardButton("🏠 Главная")
                    }
                }, resizeKeyboard: true, oneTimeKeyboard: false);

        public static ReplyKeyboardMarkup RegistrationMarkup =
               new ReplyKeyboardMarkup(new[]
               { 
                   new KeyboardButton("🔑 Регистрация") 
               },
               resizeKeyboard: true, oneTimeKeyboard: true);

        public static ReplyKeyboardMarkup SettingsMarkup = new ReplyKeyboardMarkup(
            new[]
            {
                    new[]
                    {
                        new KeyboardButton("👤 Мой профиль")
                    },
                    new[]
                    {
                        new KeyboardButton("⚥  Сменить пол"),
                        new KeyboardButton("🙌 Комплимент дня"),
                    },
                    new[]
                    {
                        new KeyboardButton("💸 Ставка по умолчанию")
                    },
                    new[]
                    {
                        new KeyboardButton("🏠 Главная")
                    }
                }, resizeKeyboard: true, oneTimeKeyboard: false);
    }
}
