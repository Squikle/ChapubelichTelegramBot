using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Entities;
using ChapubelichBot.Types.Managers;
using ChapubelichBot.Types.Managers.MessagesSender;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Group = ChapubelichBot.Types.Entities.Group;
using User = ChapubelichBot.Types.Entities.User;

namespace ChapubelichBot.CommandEntities.RegexCommands
{
    class PersonRollRegex : RegexCommand
    {
        public override string Pattern => @"!^\/? ?(.*[^ ]) ?дня\??(@ChapubelichBot)?$";

        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();

            Group group = await dbContext.Groups.Include(g => g.Users)
                .Include(g => g.GroupDailyPerson)
                .ThenInclude(gpd => gpd.User)
                .FirstOrDefaultAsync(g => g.GroupId == message.Chat.Id);
            if (group == null)
                return;

            if (group.GroupDailyPerson != null)
            {
                await SendAlreadyExistMessageAsync(client, group, message);
                return;
            }
            string regexName = Regex.Match(message.Text, Pattern, RegexOptions.IgnoreCase).Groups[1].Value;

            int maxDailyPersonNameLenght = ChapubelichClient.GetConfig().GetValue<int>("AppSettings:MaxDailyPersonNameLenght");
            if (regexName.Length > maxDailyPersonNameLenght)
            {
                await client.TrySendTextMessageAsync(message.Chat.Id, 
                    $"Кличка не может быть длиннее <b>{maxDailyPersonNameLenght}</b> символов",
                    replyToMessageId: message.MessageId);
                return;
            }

            List<User> users = group.Users;
            if (!users.Any())
            {
                await client.TrySendTextMessageAsync(message.Chat.Id,
                    "Не удалось получить пользователей чата 😞",
                    replyToMessageId: message.MessageId);
                return;
            }

            Random rand = new Random();
            User rolledUser = users[rand.Next(0, users.Count)];
            ChatMember member = await client.GetChatMemberAsync(group.GroupId, rolledUser.UserId);
            if (member == null)
                return;

            string rolledUserFirstName = member.User.FirstName;

            group.GroupDailyPerson = new GroupDailyPerson
            {
                User = rolledUser,
                RolledName = regexName,
                Group = group
            };
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return;
            }

            Message answerMessage = await client.TrySendTextMessageAsync(message.Chat.Id,
                $"🎉 <i><a href=\"tg://user?id={member.User.Id}\">{rolledUserFirstName}</a></i> <b>\"{regexName}\"</b> дня 🎉",
                parseMode: ParseMode.Html,
                replyToMessageId: message.MessageId);
            Task sendingSticker = client.TrySendStickerAsync(message.Chat.Id,
                GetRandomSticker());

            group.GroupDailyPerson.RollMessageId = answerMessage?.MessageId;
            await dbContext.SaveChangesAsync();
            await sendingSticker;
        }

        private InputOnlineFile GetRandomSticker()
        {
            Random rand = new Random();
            string[] urls = 
            {
                "CAACAgIAAxkBAAEBm0tfuQejDkwETOCOPhJYLTMW-ZiykQACdgAD1vaIDnVjyYbuYApWHgQ", // идущий пес
                "CAACAgIAAxkBAAEBm01fuQe385-8Jg6IPBXiZgHq-cTBpQACdwAD1vaIDouXVo-wBqjRHgQ", // хлопающий пес
                "CAACAgIAAxkBAAEBm09fuQe5iSsL6lS_R2p5FjJVgxFvKwACeAAD1vaIDqSlFVObZwkVHgQ", // встряхивающий руки пес
                "CAACAgIAAxkBAAEBm1FfuQffhx2i15eODiTPRctEkaC4CAACeQAD1vaIDuWEUfC2GzThHgQ", // покерфейс пес
                "CAACAgIAAxkBAAEBm1NfuQftGm8Geslemgpfs9VCJYfvdQACegAD1vaIDnOjsBRIL7hnHgQ", // улыбающаяся пепа
                "CAACAgIAAxkBAAEBm1dfuQgBrp-mGC54si5Ve5qkeS9mFAACfAAD1vaIDlJVT57SnwS2HgQ", // уходящая пепа
                "CAACAgIAAxkBAAEBm1lfuQgd4kD-foVLu9pT90M2k2UZIwACfQAD1vaIDjXMpn-IxaD7HgQ", // хлопающая пепа
                "CAACAgIAAxkBAAEBm1tfuQggmhAxEBDEMabVBe9SS4Q-egACfgAD1vaIDnMUpmEKBiiPHgQ", // катающийся кот
                "CAACAgIAAxkBAAEBm11fuQg7FKWOCrr7Xqi015JHiqHbJgACfwAD1vaIDnJYJvMdU1rxHgQ", // пьющая пепа
                "CAACAgIAAxkBAAEBm19fuQg9hzH17oyw49oDbJL4CTRa9AACgAAD1vaIDpgnXD25Jp5yHgQ", // поедающая попкорн вишня
                "CAACAgIAAxkBAAEBm2FfuQhh2oxsCFvziPMSzLbKrbyOUAACgQAD1vaIDv4Lcl8VN3XrHgQ", // уточка типа хз
                "CAACAgIAAxkBAAEBm2NfuQhu67h2NJgMJ64QUEuL-r3kZAACggAD1vaIDoWynpZxDzH0HgQ"  // уходащя в пещеру пепа
            };

            return new InputOnlineFile(urls[rand.Next(0, urls.Length)]);
        }

        private async Task SendAlreadyExistMessageAsync(ITelegramBotClient client, Group group, Message message)
        {
            ChatMember alreadyRolledMember = await client.GetChatMemberAsync(group.GroupId, group.GroupDailyPerson.UserId);
            int replyMessageId = group.GroupDailyPerson.RollMessageId ?? 0;
            await client.TrySendTextMessageAsync(message.Chat.Id,
                $"<i>{alreadyRolledMember.User.FirstName}</i> уже <b>\"{group.GroupDailyPerson.RolledName}\"</b> дня",
                parseMode: ParseMode.Html,
                replyToMessageId: replyMessageId);
        }
    }
}
