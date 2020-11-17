using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChapubelichBot.Database;
using ChapubelichBot.Init;
using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class TheftRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *\-(\d+)( +([\s\S]+))?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            var markedUser = message.ReplyToMessage?.From;

            if (markedUser == null
                || markedUser.Id == message.From.Id
                || markedUser.Id == client.BotId)
                return;

            await using var db = new ChapubelichdbContext();
            var thief = db.Users.FirstOrDefault(x => x.UserId == message.From.Id); 
            var theftFrom = db.Users.FirstOrDefault(x => x.UserId == markedUser.Id);

            if (theftFrom == null)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    $"Пользователь <a href=\"tg://user?id={markedUser.Id}\">{markedUser.FirstName}</a> еще не зарегестрировался😔",
                    Telegram.Bot.Types.Enums.ParseMode.Html,
                    replyToMessageId: message.MessageId);
                return;
            }

            IConfiguration config = Bot.GetConfig();

            if (thief == null)
                return;
            if (thief.LastMoneyTheft.AddSeconds(config.GetValue<int>("AppSettings:TheftReloadDuration")) > DateTime.UtcNow)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Ты воруешь слишком часто🥺",
                    replyToMessageId: message.MessageId);
                return;
            }

            Match match = Regex.Match(message.Text, Pattern, RegexOptions.IgnoreCase);
            string theftSumString = match.Groups[1].Value;

            long maxTheftSum = config.GetValue<long>("AppSettings:MaxTheftSum");

            if (!long.TryParse(theftSumString, out long theftSum)
                || theftSum > maxTheftSum)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Не надо так много воровать🥺",
                    replyToMessageId: message.MessageId);
                return;
            }

            if (theftSum == 0)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Прикольно придумал 👍",
                    replyToMessageId: message.MessageId);
                return;
            }

            string attachedMessage = match.Groups[3].Value;

            Random rand = new Random();
            int randCase = rand.Next(0, 100);

            string resultMessage = String.Empty;

            if (randCase < 20)
            {
                if (theftFrom.Balance >= theftSum)
                {
                    resultMessage = 
                        $"<a href=\"tg://user?id={thief.UserId}\">{thief.FirstName}</a> украл {theftSum.ToMoneyFormat()} 💵" +
                        $" у <a href=\"tg://user?id={theftFrom.UserId}\">{theftFrom.FirstName}</a>" +
                        $"\nТеперь у {thief.FirstName} {(thief.Balance + theftSum).ToMoneyFormat()} 💰";
                    if (!string.IsNullOrEmpty(attachedMessage) && attachedMessage.Length < 50)
                        resultMessage += $"\nПодпись: {attachedMessage}";


                    theftFrom.Balance -= theftSum;
                    thief.Balance += theftSum;
                }
                else
                {
                    resultMessage = 
                        $"<a href=\"tg://user?id={thief.UserId}\">{thief.FirstName}</a> попытался украсть {theftSum.ToMoneyFormat()} 💵" +
                        $" у <a href=\"tg://user?id={theftFrom.UserId}\">{theftFrom.FirstName}</a>" +
                        $"\nНо у {(theftFrom.Gender ? "него" : "неё")} было всего {theftFrom.Balance.ToMoneyFormat()} 💰" +
                        $"\nТеперь у {thief.FirstName} {(thief.Balance + theftFrom.Balance).ToMoneyFormat()} 💰";
                    if (!string.IsNullOrEmpty(attachedMessage) && attachedMessage.Length < 50)
                        resultMessage += $"\nПодпись: {attachedMessage}";

                    theftFrom.Balance -= theftFrom.Balance;
                    thief.Balance += theftFrom.Balance;
                }
            }
            else if (randCase < 33)
            {
                float stealPercentage = rand.Next(20, 76) * 0.01f;
                long reducedTheftSum = (long)(theftSum * stealPercentage);

                if (theftFrom.Balance >= reducedTheftSum)
                {
                    resultMessage =
                        $"<a href=\"tg://user?id={thief.UserId}\">{thief.FirstName}</a> попытался украсть {theftSum.ToMoneyFormat()} 💵" +
                        $" у <a href=\"tg://user?id={theftFrom.UserId}\">{theftFrom.FirstName}</a>" +
                        $"\nНо получилось украсть только {reducedTheftSum} 💵" +
                        $"\nТеперь у {thief.Username} {(thief.Balance + reducedTheftSum).ToMoneyFormat()} 💰";
                    if (!string.IsNullOrEmpty(attachedMessage) && attachedMessage.Length < 50)
                        resultMessage += $"\nПодпись: {attachedMessage}";

                    theftFrom.Balance -= reducedTheftSum;
                    thief.Balance += reducedTheftSum;
                }
                else
                {
                    resultMessage = 
                        $"<a href=\"tg://user?id={thief.UserId}\">{thief.FirstName}</a> попытался украсть {theftSum.ToMoneyFormat()} 💵" +
                        $" у <a href=\"tg://user?id={theftFrom.UserId}\">{theftFrom.FirstName}</a>" +
                        $"\nНо у {(theftFrom.Gender ? "него" : "неё")} было всего {theftFrom.Balance.ToMoneyFormat()} 💰" +
                        $"\nТеперь у {thief.Username} {(thief.Balance + theftFrom.Balance).ToMoneyFormat()} 💰";
                    if (!string.IsNullOrEmpty(attachedMessage) && attachedMessage.Length < 50)
                        resultMessage += $"\nПодпись: {attachedMessage}";

                    theftFrom.Balance -= theftFrom.Balance;
                    thief.Balance += theftFrom.Balance;
                }
            }
            else
            {
                resultMessage =
                    $"<a href=\"tg://user?id={thief.UserId}\">{thief.FirstName}</a> попытался украсть {theftSum.ToMoneyFormat()} 💵" +
                    $" у <a href=\"tg://user?id={theftFrom.UserId}\">{theftFrom.FirstName}</a>" +
                    $"\nНо у {thief.FirstName} ничего не получилось 😇";
                if (!string.IsNullOrEmpty(attachedMessage) && attachedMessage.Length < 50)
                    resultMessage += $"\n{thief.FirstName} хотел сказать: {attachedMessage}";
            }

            if (string.IsNullOrEmpty(resultMessage))
                return;

            thief.LastMoneyTheft = DateTime.UtcNow;
            db.SaveChanges();
            
            await client.TrySendTextMessageAsync(
                message.Chat.Id,
                resultMessage,
                Telegram.Bot.Types.Enums.ParseMode.Html,
                replyToMessageId: message.MessageId);
        }
    }
}
