using Telegram.Bot.Types.ReplyMarkups;

namespace ChapubelichBot.Types.Statics
{
    static class ReplyKeyboardsStatic
    {
        public static ReplyKeyboardMarkup MainMarkup = new ReplyKeyboardMarkup(
            new KeyboardButton[][]
                {
                    new KeyboardButton[] 
                    { 
                        new KeyboardButton("💰 Баланс"),
                        new KeyboardButton("🕹 Игры")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("⚙️ Настройки")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("💵 Ежедневная награда")
                    },
                    new KeyboardButton[] 
                    { 
                        new KeyboardButton("❓ Помощь") 
                    }
                }, resizeKeyboard: true, oneTimeKeyboard: false);

        public static ReplyKeyboardMarkup GameMarkup = new ReplyKeyboardMarkup(
            new KeyboardButton[][]
                {
                    new KeyboardButton[]
                    {
                        new KeyboardButton(RouletteTableStatic.Name)
                    },
                    new KeyboardButton[]
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
            new KeyboardButton[][]
                {
                    new KeyboardButton[]
                    {
                        new KeyboardButton("👤 Мой профиль")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("⚥  Сменить пол")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("💸 Ставка по умолчанию")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("🏠 Главная")
                    }
                }, resizeKeyboard: true, oneTimeKeyboard: false);
    }
}
