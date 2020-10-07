using Chapubelich.Abstractions;
using Chapubelich.ChapubelichBot.Statics;
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
            ReplyKeyboardMarkup registrationMarkup =
               new ReplyKeyboardMarkup(new[] 
               { new KeyboardButton("\U0001F511 Register") }, 
               resizeKeyboard: false, oneTimeKeyboard: true);

            await client.TrySendTextMessageAsync(
                message.Chat.Id,
                "Велкам!\n" +
                "Чтобы поздороваться с ботом - отправь команду /hello\n" +
                "По поводу возникших вопросов - @Squikle\n" +
                "Для начала нужно зарегестрироваться. Для этого нажми на кнопку снизу\U0001F609",
                replyMarkup: registrationMarkup);
            return;
        }
    }
}
