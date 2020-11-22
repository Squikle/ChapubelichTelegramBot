using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.Commands
{
    public class Start : Command
    {
        public override string Name => "/start";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(
                message.Chat.Id,
                "Привет!\n" +
                "По поводу возникших вопросов - @Squikle\n" +
                "Для начала нужно зарегестрироваться. Для этого нажми на кнопку снизу👇",
                replyMarkup: ReplyKeyboards.RegistrationMarkup);
        }
    }
}
