using System;
using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.RegexCommands
{
    class QuestionGameRegex : RegexCommand
    {
        public override string Pattern => @"^\/? *@ChapubelichBot .+\?$";

        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            Random random = new Random();

            string[][] answerStrings =
            {
                new[]
                {
                    "Да",
                    "Конечно",
                    "Определенно",
                    "Справедливо",
                    "Почему бы и нет?"
                },
                new[]
                {
                    "Нет",
                    "Ни в коем случае",
                    "Никак нет",
                    "Возможно, но нет",
                    "Та ну не"
                },
                new[]
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
                new[]
                {
                    "✔️",
                    "✅",
                    "☑️",
                    "👍",
                    "👌",
                    "💯"
                },
                new[]
                {
                    "👎",
                    "\U0001F645",                               //🙅
                    "\U0001F645\U0000200D\U00002640\U0000FE0F", //🙅‍♀
                    "❌",
                    "⛔",
                    "🙃"
                },
                new[]
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
