﻿using Chapubelich.Abstractions;
using Chapubelich.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Chapubelich.Chatting.Commands
{
    class HelpCommand : Command
    {
        public override string Name => "\U00002753 Помощь";

        public override async void Execute(Message message, ITelegramBotClient client)
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
                "                   🔸Пример: \"бот=молодец?\" .\n" +
                "         🔹Формат команды для вопроса отмечая сообщение - \"=*чему-то*?\" .\n\n" +

                "💬Так же некоторые команды можно посмотреть нажав на \"слеш\" на поле ввода.\n\n" +

                "\U0001F468\U0000200D\U0001F4BBПо дополнительным вопросам и предложениям - @Squikle .",
                replyToMessageId: message.MessageId,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
    }
}
