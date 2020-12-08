using System;
using System.Linq;
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
using User = ChapubelichBot.Types.Entities.Users.User;

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
            int theftCoolDownDuration = config.GetValue<int>("UserSettings:TheftCoolDownDuration");
            if (!CanUserTheft(thief, theftCoolDownDuration))
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

            string resultMessage = string.Empty;
            long stolenSum = 0;

            if (randCase < fullChance)
            {
                stolenSum = theftSum;
                resultMessage =
                    $"<i><a href=\"tg://user?id={thief.UserId}\">{message.From.FirstName}</a></i> украл <b>{stolenSum.ToMoneyFormat()}</b> 💵" +
                    $" у <i><a href=\"tg://user?id={theftFrom.UserId}\">{markedUser.FirstName}</a></i>";
            }
            else if (randCase < partialChance)
            {
                float stealPercentage = rand.Next(20, 76) * 0.01f;
                stolenSum = (long)(theftSum * stealPercentage);

                resultMessage =
                    $"<i><a href=\"tg://user?id={thief.UserId}\">{message.From.FirstName}</a></i> попытался украсть <b>{theftSum.ToMoneyFormat()}</b> 💵" +
                    $" у <i><a href=\"tg://user?id={theftFrom.UserId}\">{markedUser.FirstName}</a></i>" +
                    $"\nНо получилось украсть только <b>{stolenSum.ToMoneyFormat()}</b> 💵";
            }

            if (stolenSum <= 0)
            {
                resultMessage =
                    $"<i><a href=\"tg://user?id={thief.UserId}\">{message.From.FirstName}</a></i> попытался украсть <b>{theftSum.ToMoneyFormat()}</b> 💵" +
                    $" у <i><a href=\"tg://user?id={theftFrom.UserId}\">{markedUser.FirstName}</a></i>" +
                    $"\nНо у <i>{message.From.FirstName}</i> ничего не получилось 😇";
            }
            else if (theftFrom.Balance < stolenSum)
            {
                stolenSum = theftFrom.Balance;

                resultMessage =
                        $"<i><a href=\"tg://user?id={thief.UserId}\">{message.From.FirstName}</a></i> попытался украсть <b>{theftSum.ToMoneyFormat()}</b> 💵" +
                        $" у <i><a href=\"tg://user?id={theftFrom.UserId}\">{markedUser.FirstName}</a></i>" +
                        $"\nНо у <i>{(theftFrom.Gender ? "него" : "неё")}</i> было всего <b>{theftFrom.Balance.ToMoneyFormat()}</b> 💰";
            }

            if (string.IsNullOrEmpty(resultMessage))
                return;

            thief.UserTheft ??= new UserTheft
            {
                User = thief
            };
            thief.UserTheft.LastMoneyTheft = DateTime.UtcNow;
            
            if (stolenSum > 0)
            {
                theftFrom.Balance -= stolenSum;
                thief.Balance += stolenSum;
                bool saved = false;
                while (!saved)
                {
                    try
                    {
                        await dbContext.SaveChangesAsync();
                        saved = true;
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        foreach (var entry in ex.Entries)
                        {
                            if (entry.Entity is User user)
                            {
                                Console.WriteLine("Конфликт параллелизма для баланса пользователя (TheftRegex)");
                                await entry.ReloadAsync();

                                if (user.UserId == theftFrom.UserId)
                                    user.Balance -= stolenSum;
                                else user.Balance += stolenSum;
                                if (CanUserTheft(thief, theftCoolDownDuration)) continue;
                                Console.WriteLine("Повторная попытка украсть деньги");
                                return;
                            }
                            if (entry.Entity is UserTheft)
                            {
                                Console.WriteLine("Повторная попытка украсть деньги");
                                return;
                            }
                        }
                    }
                    catch (DbUpdateException)
                    {
                        Console.WriteLine("Повторное добавление вора");
                        return;
                    }
                }

                resultMessage += $"\nТеперь у <i>{message.From.FirstName}</i> <b>{(thief.Balance).ToMoneyFormat()}</b> 💰";
                if (!string.IsNullOrEmpty(attachedMessage) && attachedMessage.Length < 50)
                    resultMessage += $"\nПодпись: <i>\"{attachedMessage}\"</i>";
            }
            else
            {
                if (!string.IsNullOrEmpty(attachedMessage) && attachedMessage.Length < 50)
                    resultMessage += $"\n<i>{(theftFrom.Gender ? "он</i> хотел" : "она</i> хотела")} сказать: <i>\"{attachedMessage}\"</i>";
            }

            await client.TrySendTextMessageAsync(
                message.Chat.Id,
                resultMessage,
                Telegram.Bot.Types.Enums.ParseMode.Html,
                replyToMessageId: message.MessageId);
        }
        private bool CanUserTheft(User thief, int theftCoolDownDuration)
        {
            return thief.UserTheft == null ||
                   thief.UserTheft.LastMoneyTheft.AddSeconds(theftCoolDownDuration) < DateTime.UtcNow;
        }
    }
}
