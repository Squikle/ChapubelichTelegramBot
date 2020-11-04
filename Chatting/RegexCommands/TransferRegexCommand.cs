using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Database;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class TransferRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *\+(\d{1,3})( +([\s\S]+))?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            var markedUser = message.ReplyToMessage?.From;

            Match match = Regex.Match(message.Text, Pattern, RegexOptions.IgnoreCase);
            string transferSumString = match.Groups[1].Value;
            string attachedMessage = match.Groups[3].Value;

            if (!Int32.TryParse(transferSumString, out int transferSum) ||
                markedUser == null ||
                markedUser == message.From ||
                transferSum == 0 ||
                markedUser.Id == client.BotId)
                return;

            await using var db = new ChapubelichdbContext();
            var transferTo = db.Users.FirstOrDefault(x => x.UserId == markedUser.Id);
            var transferFrom = db.Users.FirstOrDefault(x => x.UserId == message.From.Id);

            if (null == transferTo)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    $"Пользователь <a href=\"tg://user?id={markedUser.Id}\">{markedUser.FirstName}</a> еще не зарегестрировался\U0001F614",
                    Telegram.Bot.Types.Enums.ParseMode.Html,
                    replyToMessageId: message.MessageId);
                return;
            }

            if (transferFrom != null && transferFrom.Balance >= transferSum)
            {
                string genderWord = transferTo.Gender ? "него" : "неё";

                transferFrom.Balance -= transferSum;
                transferTo.Balance += transferSum;

                string resultMessage = $"{transferSum.ToMoneyFormat()} 💵 переданы пользователю <a href=\"tg://user?id={transferTo.UserId}\">" +
                                       $"{transferTo.FirstName}</a>\nТеперь у {genderWord} {transferTo.Balance.ToMoneyFormat()}\U0001F4B0\n";
                if (!string.IsNullOrEmpty(attachedMessage) && attachedMessage.Length < 50)
                    resultMessage += $"Подпись: {attachedMessage}";

                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    resultMessage,
                    Telegram.Bot.Types.Enums.ParseMode.Html,
                    replyToMessageId: message.MessageId);

                await db.SaveChangesAsync();
                return;
            }

            await client.TrySendTextMessageAsync(
                message.Chat.Id,
                "У тебя недостаточно средств для перевода средств😔",
                replyToMessageId: message.MessageId);
        }
    }
}
 