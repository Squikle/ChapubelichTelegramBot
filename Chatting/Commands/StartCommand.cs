using Chapubelich.Abstractions;
using Chapubelich.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chapubelich.Chatting.Commands
{
    public class StartCommand : Command
    {
        public override string Name => "/start";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            var keyboard = new[] { new KeyboardButton("\U0001F511 Register") };
            var markup = new ReplyKeyboardMarkup(keyboard, resizeKeyboard: false, oneTimeKeyboard: true);

            await client.TrySendTextMessageAsync(
                message.Chat.Id,
                "Велкам!\nЧтобы поздороваться с ботом - отправь команду /hello\nПо поводу возникших вопросов - @Squikle\nДля начала нужно зарегестрироваться. Для этого нажми на кнопку снизу\U0001F609",
                replyMarkup: markup
                );
            return;
        }
    }
}
