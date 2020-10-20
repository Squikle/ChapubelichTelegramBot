using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;

namespace ChapubelichBot.Chatting.Commands
{
    class HelpCommand : Command
    {
        public override string Name => "❓ Помощь";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(
                message.Chat.Id,
                "📖Доступные команды бота:\n" +
                "     💠Общие команды:\n" +
                "         🔹Если что-то пошло не так - \\start .\n" +
                "         🔹Поздороваться с ботом - \\hello .\n" +
                "         🔹Узнать ваш баланс - баланс, б, счет, balance .\n" +
                "         🔹Переслать средства пользователю - в ответ на сообщение пользователя отправить сообщение в формате \"+*сумма перевода*\" " +
                "(максимальная транзакция - до 10000).\n" +
                "                   🔸Пример: \"+100\".\n\n" +

                "     💠Рулетка:\n" +
                "          🔹Начать рулетку - рулетка, roulette .\n" +
                "          🔹Поставить ставку - сообщение в формате \"*сумма ставки**цвет*\" (максимальная ставка - до 10000 монет).\n" +
                "                   🔸Пример: \"100 к\", \"100 черное\".\n" +
                "         🔹Отменить ставку - отмена, cancel.\n" +
                "         🔹Посмотреть свои ставки - (мои) ставки, (my) bets .\n" +
                "         🔹Крутить барабан - го, ролл, погнали, крути, roll .\n\n" +

                "     💠Задать боту вопрос:\n" +
                "         🔹Формат команды для вопроса - \"*что-то*=*чему-то*?\" .\n" +
                "                   🔸Пример: \"бот=молодец?\" .\n\n" +

                "💬Так же некоторые команды можно посмотреть нажав на \"слеш\" на поле ввода.\n\n" +

                "\U0001F468\U0000200D\U0001F4BBПо поводу ошибок, дополнительных вопросов и предложениях - @Squikle .",
                replyToMessageId: message.MessageId,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
    }
}
