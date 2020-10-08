using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Database;
using ChapubelichBot.Types.Extensions;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class TransferRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *\+(\d{1,3})(@ChapubelichBot)?$";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            var markedUser = message?.ReplyToMessage?.From;
            string transferSumString = Regex.Match(message.Text, Pattern, RegexOptions.IgnoreCase).Groups[1].Value;

            if (!Int32.TryParse(transferSumString, out int transferSum) || null == markedUser)
                return;

            if (transferSum == 0)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    $"Вы не можете передать {transferSum} 💵 \U0001F614",
                    replyToMessageId: message.MessageId);
                return;
            }

            using (var db = new ChapubelichdbContext())
            {
                var transferTo = db.Users.FirstOrDefault(x => x.UserId == markedUser.Id);
                var transferFrom = db.Users.FirstOrDefault(x => x.UserId == message.From.Id);

                if (null == transferTo)
                {
                    await client.TrySendTextMessageAsync(
                        message.Chat.Id,
                        $"Пользователь <<a href=\"tg://user?id={transferTo.UserId}\">{transferTo.FirstName}</a> еще не зарегестрировался\U0001F614",
                        Telegram.Bot.Types.Enums.ParseMode.Html,
                        replyToMessageId: message.MessageId);
                    return;
                }

                if (transferTo == transferFrom)
                    return;

                if (transferFrom.Balance >= transferSum)
                {
                    string genderWord = transferTo.Gender ? "него" : "неё";

                    transferFrom.Balance -= transferSum;
                    transferTo.Balance += transferSum;
                    await client.TrySendTextMessageAsync(
                        message.Chat.Id,
                        $"Вы передали {transferSum} 💵 пользователю <a href=\"tg://user?id={transferTo.UserId}\">" +
                        $"{transferTo.FirstName}</a>\nТеперь у {genderWord} {transferTo.Balance}\U0001F4B0",
                        Telegram.Bot.Types.Enums.ParseMode.Html,
                        replyToMessageId: message.MessageId);

                    db.SaveChanges();
                    return;
                }

                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    $"У вас недостаточно средств\U0001F614",
                    replyToMessageId: message.MessageId);
            }
        }
    }
}
 