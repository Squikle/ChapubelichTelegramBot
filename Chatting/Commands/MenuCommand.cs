using Chapubelich.Abstractions;
using Chapubelich.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chapubelich.Chatting.Commands
{
    class MenuCommand : Command
    {
        public override string Name => "\U0001F3E0 Главная";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            var keyboard = new[] {
                new KeyboardButton("\U0001F4B0 Баланс"),
                new KeyboardButton("\U0001F579 Игры")
            };
            var markup = new ReplyKeyboardMarkup(keyboard, resizeKeyboard: false, oneTimeKeyboard: false);

            await client.TrySendTextMessageAsync(
                        message.Chat.Id,
                        "\U0001F3E0 Главное меню!",
                        replyMarkup: markup
                        );
        }
    }
}
