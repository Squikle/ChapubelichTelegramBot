﻿using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Extensions;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChapubelichBot.CommandEntities.RegexCommands
{
    class TransferRegex : RegexCommand
    {
        public override string Pattern => @"^\/? *\+(\d+)( +([\s\S]+))?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            var markedUser = message.ReplyToMessage?.From;

            if (markedUser == null 
             || markedUser.Id == message.From.Id 
             || markedUser.Id == client.BotId)
                return;

            Match match = Regex.Match(message.Text, Pattern, RegexOptions.IgnoreCase);
            string transferSumString = match.Groups[1].Value;

            long maxTransferSum = ChapubelichClient.GetConfig().GetValue<long>("AppSettings:MaxTransferSum");

            if (!long.TryParse(transferSumString, out long transferSum)
                || transferSum > maxTransferSum)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    $"Вы не можете передать больше <b>{maxTransferSum}</b> 💵 за раз",
                    replyToMessageId: message.MessageId,
                    parseMode: ParseMode.Html);
                return;
            }
            if (transferSum == 0)
                return;

            string attachedMessage = match.Groups[3].Value;

            await using var db = new ChapubelichdbContext();
            var transferTo = db.Users.FirstOrDefault(x => x.UserId == markedUser.Id);
            var transferFrom = db.Users.FirstOrDefault(x => x.UserId == message.From.Id);

            if (transferTo == null)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    $"Пользователь <a href=\"tg://user?id={markedUser.Id}\">{markedUser.FirstName}</a> еще не зарегестрировался\U0001F614",
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);
                return;
            }

            if (transferFrom != null && transferFrom.Balance >= transferSum)
            {
                string genderWord = transferTo.Gender ? "него" : "неё";

                transferFrom.Balance -= transferSum;
                transferTo.Balance += transferSum;

                string resultMessage = $"<b>{transferSum.ToMoneyFormat()}</b> 💵 переданы пользователю <a href=\"tg://user?id={transferTo.UserId}\">" +
                                       $"{markedUser.FirstName}</a>\nТеперь у {genderWord} <b>{transferTo.Balance.ToMoneyFormat()}</b>\U0001F4B0";
                if (!string.IsNullOrEmpty(attachedMessage) && attachedMessage.Length < 50)
                    resultMessage += $"\nПодпись: {attachedMessage}";

                db.SaveChanges();

                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    resultMessage,
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);
                return;
            }

            await client.TrySendTextMessageAsync(
                message.Chat.Id,
                "У тебя недостаточно средств для перевода средств😔",
                replyToMessageId: message.MessageId);
        }
    }
}
 