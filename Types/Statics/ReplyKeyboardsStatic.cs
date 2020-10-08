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
                        new KeyboardButton("\U0001F4B0 Баланс"),
                        new KeyboardButton("\U0001F579 Игры")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("\U00002699 Настройки")
                    },
                    new KeyboardButton[] 
                    { 
                        new KeyboardButton("\U00002753 Помощь") 
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
                        new KeyboardButton("\U0001F3E0 Главная")
                    }
                }, resizeKeyboard: true, oneTimeKeyboard: false);

        public static ReplyKeyboardMarkup RegistrationMarkup =
               new ReplyKeyboardMarkup(new[]
               { 
                   new KeyboardButton("\U0001F511 Регистрация") 
               },
               resizeKeyboard: true, oneTimeKeyboard: true);

        public static ReplyKeyboardMarkup SettingsMarkup = new ReplyKeyboardMarkup(
            new KeyboardButton[][]
                {
                    new KeyboardButton[]
                    {
                        new KeyboardButton("\U0001F464 Мой профиль")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("\U000026A5 Сменить пол")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("\U0001F3E0 Главная")
                    }
                }, resizeKeyboard: true, oneTimeKeyboard: false);
    }
}
