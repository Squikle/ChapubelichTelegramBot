using Telegram.Bot.Types.ReplyMarkups;

namespace Chapubelich.ChapubelichBot.Statics
{
    public static class ReplyKeyboards
    {
        public static ReplyKeyboardMarkup mainMenuMarkup = 
            new ReplyKeyboardMarkup(new[] { 
                new KeyboardButton("\U0001F4B0 Баланс"),  
                new KeyboardButton("\U0001F579 Игры") 
            }, resizeKeyboard: false, oneTimeKeyboard: false);
    }
}
