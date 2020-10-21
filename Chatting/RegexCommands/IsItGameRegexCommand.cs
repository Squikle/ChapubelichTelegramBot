using System;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using ChapubelichBot.Types.Abstractions;
using System.Threading.Tasks;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class IsItGameRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *@ChapubelichBot .+\?$";

        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            if (message.From.Id == 443763853)
            {
                await client.TrySendTextMessageAsync(message.Chat.Id, "Иди нахуй😉", replyToMessageId: message.MessageId);
                return;
            }

            Random random = new Random();

            string[][] answerStrings =
            {
                new string[]
                {
                    "Да",
                    "Конечно",
                    "Определенно",
                    "Справедливо",
                    "Почему бы и нет?"
                },
                new string[]
                {
                    "Нет",
                    "Ни в коем случае",
                    "Никак нет",
                    "Возможно, но нет",
                    "Та ну не"
                },
                new string[]
                {
                    "Вряд ли",
                    "Может быть",
                    "Не знаю",
                    "фифтифифти",
                    "смотря кто спрашивает"
                }
            };
            string[][] emojis =
            {
                new string[]
                {
                    "✔️",
                    "✅",
                    "☑️",
                    "👍",
                    "👌",
                    "💯"
                },
                new string[]
                {
                    "👎",
                    "\U0001F645",                               //🙅
                    "\U0001F645\U0000200D\U00002640\U0000FE0F", //🙅‍♀
                    "❌",
                    "⛔",
                    "🙃"
                },
                new string[]
                {
                    "\U0001F937\U0000200D\U00002640\U0000FE0F", //🤷‍♀️
                    "\U0001F937\U0000200D\U00002642\U0000FE0F"  //🤷‍♂️
                }
            };

            int numberOfAnswerType = random.Next(0, answerStrings.Length);

            string[] answerType = answerStrings[numberOfAnswerType];
            string[] emojiType = emojis[numberOfAnswerType];

            string answerString = answerType[random.Next(0, answerType.Length)] + emojiType[random.Next(0, emojiType.Length)];

            await client.TrySendTextMessageAsync(message.Chat.Id, answerString, replyToMessageId: message.MessageId);
        }
    }
}
