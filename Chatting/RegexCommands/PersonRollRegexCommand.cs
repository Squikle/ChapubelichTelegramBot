﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChapubelichBot.Database;
using ChapubelichBot.Database.Models;
using ChapubelichBot.Init;
using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Group = ChapubelichBot.Database.Models.Group;
using User = ChapubelichBot.Database.Models.User;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class PersonRollRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? ?(.*[^ ]) ?дня(@ChapubelichBot)?$";

        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await using var db = new ChapubelichdbContext();

            Group group = db.Groups.Include(g => g.Users)
                .Include(g => g.GroupDailyPerson)
                .ThenInclude(gpd => gpd.User)
                .FirstOrDefault(g => g.GroupId == message.Chat.Id);
            if (group == null)
                return;

            if (group.GroupDailyPerson != null)
            {
                ChatMember alreadyRolledMember = await client.GetChatMemberAsync(group.GroupId, group.GroupDailyPerson.UserId);
                await client.TrySendTextMessageAsync(message.Chat.Id,
                    $"<a href=\"tg://user?id={group.GroupDailyPerson.UserId}\">{alreadyRolledMember.User.FirstName}</a> " +
                    $"уже {group.GroupDailyPerson.RolledName} дня",
                    parseMode: ParseMode.Html,
                    replyToMessageId: message.MessageId);
                return;
            }
            string regexName = Regex.Match(message.Text, Pattern, RegexOptions.IgnoreCase).Groups[1].Value;

            int maxDailyPersonNameLenght = Bot.GetConfig().GetValue<int>("AppSettings:MaxDailyPersonNameLenght");
            if (regexName.Length > maxDailyPersonNameLenght)
            {
                await client.TrySendTextMessageAsync(message.Chat.Id, 
                    $"Кличка не может быть длиннее {maxDailyPersonNameLenght} символов",
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

            Task sendingTaskMessage = client.TrySendTextMessageAsync(message.Chat.Id, 
                $"<a href=\"tg://user?id={member.User.Id}\">{rolledUserFirstName}</a> {regexName} дня",
                parseMode: ParseMode.Html,
                replyToMessageId: message.MessageId);
            Task sendingSticker = client.TrySendStickerAsync(message.Chat.Id,
                GetRandomSticker());

            group.GroupDailyPerson = new GroupDailyPerson
            {
                User = rolledUser,
                RolledName = regexName,
                Group = group
            };
            db.SaveChanges();

            await sendingTaskMessage;
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
    }
}