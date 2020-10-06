using Chapubelich.Abstractions;
using Chapubelich.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chapubelich.Chatting.Commands
{
    class GamesCommand : Command
    {
        public override string Name => "\U0001F579 Игры";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            var keyboard = new[] {
                new KeyboardButton("\U0001F3B0 50/50"),
                new KeyboardButton("\U0001F3E0 Главная")
            };
            var markup = new ReplyKeyboardMarkup(keyboard, resizeKeyboard: false, oneTimeKeyboard: false);
            await client.TrySendTextMessageAsync(
                        message.Chat.Id,
                        "\U0001F579 Список доступных игр!",
                        replyMarkup: markup
                        );
        }
    }
}
