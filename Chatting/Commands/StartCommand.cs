using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.Commands
{
    public class StartCommand : Command
    {
        public override string Name => "/start";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(
                message.Chat.Id,
                "Велкам!\n" +
                "Чтобы поздороваться с ботом - отправь команду /hello\n" +
                "По поводу возникших вопросов - @Squikle\n" +
                "Для начала нужно зарегестрироваться. Для этого нажми на кнопку снизу\U0001F609",
                replyMarkup: ReplyKeyboardsStatic.RegistrationMarkup);
            return;
        }
    }
}
