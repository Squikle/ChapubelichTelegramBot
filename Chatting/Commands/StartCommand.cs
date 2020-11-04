using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;

namespace ChapubelichBot.Chatting.Commands
{
    public class StartCommand : Command
    {
        public override string Name => "/start";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(
                message.Chat.Id,
                "Велкам!\n" +
                "Чтобы поздороваться с ботом - отправь команду /hello\n" +
                "По поводу возникших вопросов - @Squikle\n" +
                "Для начала нужно зарегестрироваться. Для этого нажми на кнопку снизу👇",
                replyMarkup: ReplyKeyboardsStatic.RegistrationMarkup);
        }
    }
}
