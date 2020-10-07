using Chapubelich.Abstractions;
using Chapubelich.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Chapubelich.Chatting.Games.IsHeGame
{
    class IsItGameRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/?([^\r\n\t\f\v=]*) *= *(([^\r\n\t\f\v= ]+ *)+)\?$";

        public override async void Execute(Message message, ITelegramBotClient client)
        {

            if (message.ReplyToMessage == null
                && !Regex.Match(message.Text, Pattern).Groups[1].Value.Any())
                return;

            Random random = new Random();

            string[] confirmingEmojis =
            {
                "✔️",
                "✅",
                "☑️",
                "👍",
                "👌",
                "💯"
            };
            string[] negativeEmojis =
            {
                "👎",
                "\U0001F645", // 🙅
                "\U0001F645\U0000200D\U00002640\U0000FE0F", // 🙅‍♀
                "❌",
                "⛔",
                "🙃"
            };
            string[] maybeEmojis =
            {
                "\U0001F937\U0000200D\U00002640\U0000FE0F", //🤷‍♀️
                "\U0001F937\U0000200D\U00002642\U0000FE0F" //🤷‍♂️
            };

            string answer = string.Empty;
            switch (random.Next(0,8))
            {
                case 0:
                    answer = "Да" + GetRandomEmoji(confirmingEmojis, random);
                    break;
                case 1:
                    answer = "Конечно" + GetRandomEmoji(confirmingEmojis, random);
                    break;
                case 2:
                    answer = "Определенно" + GetRandomEmoji(confirmingEmojis, random);
                    break;
                case 3:
                    answer = "Может быть" + GetRandomEmoji(maybeEmojis, random);
                    break;
                case 4:
                    answer = "Нет" + GetRandomEmoji(negativeEmojis, random);
                    break;
                case 5:
                    answer = "Ни в коем случае" + GetRandomEmoji(negativeEmojis, random);
                    break;
                case 6:
                    answer = "Никак нет" + GetRandomEmoji(negativeEmojis, random);
                    break;
                case 7:
                    answer = "Вряд ли" + GetRandomEmoji(maybeEmojis, random);
                    break;
            }

            await client.TrySendTextMessageAsync(message.Chat.Id, answer, replyToMessageId: message.MessageId);
        }

        private static string GetRandomEmoji(string[] emojiList, Random random)
        {
            return emojiList[random.Next(0, emojiList.Length)];
        }
    }
}
