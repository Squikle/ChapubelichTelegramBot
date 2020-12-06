﻿using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Entities.Users;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Managers.MessagesSender;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.RegexCommands
{
    class TheftRegex : RegexCommand
    {
        public override string Pattern => @"^\/? *\-(\d+)( +([\s\S]+))?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            var markedUser = message.ReplyToMessage?.From;

            if (markedUser == null
                || markedUser.Id == message.From.Id
                || markedUser.Id == client.BotId)
                return;

            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            var thief = await dbContext.Users.Include(u => u.UserTheft).FirstOrDefaultAsync(x => x.UserId == message.From.Id); 
            var theftFrom = await dbContext.Users.FirstOrDefaultAsync(x => x.UserId == markedUser.Id);

            if (theftFrom == null)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    $"Пользователь <i><a href=\"tg://user?id={markedUser.Id}\">{markedUser.FirstName}</a></i> еще не зарегестрировался 😔",
                    Telegram.Bot.Types.Enums.ParseMode.Html,
                    replyToMessageId: message.MessageId);
                return;
            }

            IConfiguration config = ChapubelichClient.GetConfig();

            if (thief == null)
                return;
            if (thief.UserTheft != null && thief.UserTheft.LastMoneyTheft.AddSeconds(config.GetValue<int>("UserSettings:TheftCoolDownDuration")) > DateTime.UtcNow)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Ты воруешь слишком часто 🥺",
                    replyToMessageId: message.MessageId);
                return;
            }

            Match match = Regex.Match(message.Text, Pattern, RegexOptions.IgnoreCase);
            string theftSumString = match.Groups[1].Value;

            long maxTheftSum = config.GetValue<long>("UserSettings:MaxTheftSum");

            if (!long.TryParse(theftSumString, out long theftSum)
                || theftSum > maxTheftSum)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Не надо так много воровать 🥺",
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

            int fullChance = config.GetValue<int>("UserSettings:FullTheftChance"); // шанс забрать 100%
            int partialChance = config.GetValue<int>("UserSettings:PartialTheftChance") + fullChance; // шанс забрать часть
            int coef = config.GetValue<int>("UserSettings:TheftSumCoefficient"); // влияние размера кражи на шанс (чем больше тем больше шанс украсть много) (при 30 почти не влияет, при 5 шанс падает в 2 раза при краже от 1% до 100%)

            int randCase = (int)((float)theftSum / maxTheftSum * 100 / coef) + rand.Next(0, 100);

            if (randCase > 99)
                randCase = 99;

            string resultMessage;

            if (randCase < fullChance)
            {
                if (theftFrom.Balance >= theftSum)
                {
                    resultMessage = 
                        $"<i><a href=\"tg://user?id={thief.UserId}\">{message.From.FirstName}</a></i> украл <b>{theftSum.ToMoneyFormat()}</b> 💵" +
                        $" у <i><a href=\"tg://user?id={theftFrom.UserId}\">{markedUser.FirstName}</a></i>" +
                        $"\nТеперь у <i>{message.From.FirstName}</i> <b>{(thief.Balance + theftSum).ToMoneyFormat()}</b> 💰";
                    if (!string.IsNullOrEmpty(attachedMessage) && attachedMessage.Length < 50)
                        resultMessage += $"\nПодпись: <i>\"{attachedMessage}\"</i>";


                    theftFrom.Balance -= theftSum;
                    thief.Balance += theftSum;
                }
                else
                {
                    resultMessage = 
                        $"<i><a href=\"tg://user?id={thief.UserId}\">{message.From.FirstName}</a></i> попытался украсть <b>{theftSum.ToMoneyFormat()}</b> 💵" +
                        $" у <i><a href=\"tg://user?id={theftFrom.UserId}\">{markedUser.FirstName}</a></i>" +
                        $"\nНо у <i>{(theftFrom.Gender ? "него" : "неё")}</i> было всего <b>{theftFrom.Balance.ToMoneyFormat()}</b> 💰" +
                        $"\nТеперь у <i>{message.From.FirstName}</i> <b>{(thief.Balance + theftFrom.Balance).ToMoneyFormat()}</b> 💰";
                    if (!string.IsNullOrEmpty(attachedMessage) && attachedMessage.Length < 50)
                        resultMessage += $"\nПодпись: <i>\"{attachedMessage}\"</i>";

                    theftFrom.Balance -= theftFrom.Balance;
                    thief.Balance += theftFrom.Balance;
                }
            }
            else if (randCase < partialChance)
            {
                float stealPercentage = rand.Next(20, 76) * 0.01f;
                long reducedTheftSum = (long)(theftSum * stealPercentage);

                if (theftFrom.Balance >= reducedTheftSum)
                {
                    resultMessage =
                        $"<i><a href=\"tg://user?id={thief.UserId}\">{message.From.FirstName}</a></i> попытался украсть <b>{theftSum.ToMoneyFormat()}</b> 💵" +
                        $" у <i><a href=\"tg://user?id={theftFrom.UserId}\">{markedUser.FirstName}</a></i>" +
                        $"\nНо получилось украсть только <b>{reducedTheftSum}</b> 💵" +
                        $"\nТеперь у <i>{message.From.FirstName}</i> <b>{(thief.Balance + reducedTheftSum).ToMoneyFormat()}</b> 💰";
                    if (!string.IsNullOrEmpty(attachedMessage) && attachedMessage.Length < 50)
                        resultMessage += $"\nПодпись: <i>\"{attachedMessage}\"</i>";

                    theftFrom.Balance -= reducedTheftSum;
                    thief.Balance += reducedTheftSum;
                }
                else
                {
                    resultMessage = 
                        $"<i><a href=\"tg://user?id={thief.UserId}\">{message.From.FirstName}</a></i> попытался украсть <b>{theftSum.ToMoneyFormat()}</b> 💵" +
                        $" у <i><a href=\"tg://user?id={theftFrom.UserId}\">{markedUser.FirstName}</a></i>" +
                        $"\nНо у <i>{(theftFrom.Gender ? "него" : "неё")}</i> было всего <b>{theftFrom.Balance.ToMoneyFormat()}</b> 💰" +
                        $"\nТеперь у <i>{message.From.FirstName}</i> <b>{(thief.Balance + theftFrom.Balance).ToMoneyFormat()}</b> 💰";
                    if (!string.IsNullOrEmpty(attachedMessage) && attachedMessage.Length < 50)
                        resultMessage += $"\nПодпись: <i>\"{attachedMessage}\"</i>";

                    theftFrom.Balance -= theftFrom.Balance;
                    thief.Balance += theftFrom.Balance;
                }
            }
            else
            {
                resultMessage =
                    $"<i><a href=\"tg://user?id={thief.UserId}\">{message.From.FirstName}</a></i> попытался украсть <b>{theftSum.ToMoneyFormat()}</b> 💵" +
                    $" у <i><a href=\"tg://user?id={theftFrom.UserId}\">{markedUser.FirstName}</a></i>" +
                    $"\nНо у <i>{message.From.FirstName}</i> ничего не получилось 😇";
                if (!string.IsNullOrEmpty(attachedMessage) && attachedMessage.Length < 50)
                    resultMessage += $"\n<i>{(theftFrom.Gender ? "он</i> хотел" : "она</i> хотела")} сказать: <i>\"{attachedMessage}\"</i>";
            }

            if (string.IsNullOrEmpty(resultMessage))
                return;

            thief.UserTheft ??= new UserTheft
            {
                User = thief
            };
            thief.UserTheft.LastMoneyTheft = DateTime.UtcNow;

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                Console.WriteLine("Повторная попытка украсть деньги");
                return;
            }
            catch (DbUpdateException)
            {
                Console.WriteLine("Повторное добавление вора");
                return;
            }
            
            await client.TrySendTextMessageAsync(
                message.Chat.Id,
                resultMessage,
                Telegram.Bot.Types.Enums.ParseMode.Html,
                replyToMessageId: message.MessageId);
        }
    }
}
