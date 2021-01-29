using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Managers.MessagesSender;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.Commands
{
    class Help : Command
    {
        public override string Name => "❓ Помощь";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(
                message.Chat.Id,
                "📖<b>Доступные команды бота:</b>\n" +
                "     💠<u>Общие команды</u>:\n" +
                "         🔹<b>Поздороваться с ботом</b> - \"\\hello\";\n" +
                "         🔹<b>Узнать ваш баланс</b> - \"<code>баланс/б/счет/balance/b</code>\";\n" +
                "         🔹<b>Переслать средства пользователю в чате</b> - в ответ на сообщение пользователя отправить сообщение в формате \"<code>+*сумма перевода*</code>\";\n" +
                "                   🔸<i>Пример: \"<code>+100</code>\".</i>\n" +
                "         🔹<b>Попытаться украсть средства у пользователя в чате</b> - аналогично переводу денег, но используя \"-\": \"<code>-*сумма кражи*</code>\";\n" +
                "         🔹<b>Посмотреть топ 5 богатеев чата</b> - \"<code>топ 5</code>\".\n\n" +

                "     💠<u>Рулетка:</u>\n" +
                "         🔹<b>Начать рулетку</b> - \"<code>рулетка/roulette</code>\";\n" +
                "         🔹<b>Поставить ставку</b> - \"<code>*сумма ставки* *цвет/число*</code>\"\n" +
                "                   🔸<i>Примеры: \"<code>100 к</code>\", \"<code>10 черное</code>\", \"<code>50 10</code>\", \"<code>228 1-4</code>\".</i>\n" +
                "         🔹<b>Отменить ставку</b> - \"<code>отмена/cancel</code>\";\n" +
                "         🔹<b>Посмотреть свои ставки</b> - \"<code>ставки/bets</code>\";\n" +
                "         🔹<b>Крутить барабан</b> - \"<code>го/ролл/погнали/крути/roll/go</code>\";\n" +
                "         🔹<b>Посмотреть историю последних 5 игр</b> - \"<code>лог 5</code>\".\n\n" +
                
                "     💠<u>Алиас:</u>\n" +
                "         🔹<b>Начать игру</b> - \"<code>алиас/угадайка/alias</code>\";\n" +
                "         🔹<b>Отгадать слово</b> - \"<code>.*слово*</code>\".\n\n" +
                
                "     💠<u>Leet переводчик:</u>\n" +
                "         🔹<b>Перевести текст в/из leetspeak</b> - \"<code>лит/1337/leetspeak/leet/литспик *текст для перевода*</code>\"\n" +
                "                   🔸<i>Можно явно указать сторону перевода (-l для перевода в leet, -n для перевода в латиницу): \"<code>leet -l example</code>\".</i>\n\n" +
                
                "     💠<u>Задать боту вопрос:</u>\n" +
                "         🔹<b>Формат команды для вопроса</b> - \"<code>@ChapubelichBot *вопрос*?</code>\"\n" +
                "                   🔸<i>Пример: \"<code>@ChapubelichBot я молодец?</code>\".</i>\n\n" +

                "     💠<u>Человек дня:</u>\n" +
                "         🔹<b>Определить человека дня</b> - \"<code>!*слово* дня</code>\"\n" +
                "                   🔸<i>Пример: \"<code>!молодец дня</code>\".</i>\n\n" +
                
                "💬<u>Так же некоторые команды можно посмотреть нажав на \"слеш\" ('/') на поле ввода.</u>\n\n" +
                
                "💬<u>В настройках можно:</u>\n" +
                "     💠<b>Посмотреть данные о своем профиле;</b>\n" +
                "     💠<b>Включить подписку на ежедневные комплименты;</b>\n" +
                "     💠<b>Сменить пол профиля;</b>\n" +
                "     💠<b>Изменить ставку по умолчанию.</b>\n\n" +

                "\U0001F468\U0000200D\U0001F4BBПо вопросам, предложениям и по поводу ошибок - писать @Squikle.",
                replyToMessageId: message.MessageId,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
    }
}
