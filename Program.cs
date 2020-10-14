using ChapubelichBot.Types.Statics;
using ChapubelichBot.Database;
using ChapubelichBot.Types.Extensions;
using System;
using System.Linq;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using ChapubelichBot.Init;
using User = ChapubelichBot.Database.Models.User;
using System.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ChapubelichBot
{
    class Program
    {
        private static readonly ITelegramBotClient client = Bot.Client;
        static void Main()
        {
            var me = client.GetMeAsync();
            //Console.Title = me.Username;

            Console.WriteLine($"StartReceiving...");
            client.StartReceiving();

            /*using (var db = new ChapubelichdbContext())
            {
                *//*var user1 = new User() { Gender = true, Username = "Squikle", UserId = 243857110, UserGroup = new List<UserGroup>() };

                var group1 = new Group() { Name = "only me", GroupId = -1001364770969 };
                var group2 = new Group() { Name = "test", GroupId = -1001364770965 };

                var userGroup1 = new UserGroup() { Group = group1, User = user1 };
                var userGroup2 = new UserGroup() { Group = group2, User = user1 };

                user1.UserGroup.Add(userGroup1);
                user1.UserGroup.Add(userGroup2);

                db.Groups.Add(group1);
                db.Groups.Add(group2);

                db.Users.Add(user1);

                db.SaveChangesAsync();*//*
                var li1 = db.Set<UserGroup>().Where(x => x.User.Username == "Squikle").Select(x => x.Group.Name);
                var li2 = db.Set<UserGroup>().Where(x => x.Group.Name == "Only me").Select(x => x.User.Username);

                foreach (var el in li2)
                    Console.WriteLine(el);
            }*/
            client.OnMessage += MessageProcess;
            client.OnCallbackQuery += CallbackProcess;

            Thread.Sleep(int.MaxValue);
        }


        static void MessageProcess(object sender, MessageEventArgs e)
        {
            if (null == e.Message || null == e.Message.Text)
                return;

            //var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            //if (e.Message.Date.AddHours(offset.Hours).AddMinutes(AppSettings.MessagesPeriod) > DateTime.Now)
            if (e.Message.Date.AddMinutes(AppSettings.MessagesCheckPeriod) < DateTime.UtcNow)
                return;

            Console.WriteLine("{0}: {1} | {2} ({3} | {4}):\t {5}",
                e.Message.Date.ToString("HH:mm:ss"),
                e.Message.From.Id, e.Message.From.Username,
                e.Message.Chat.Id, e.Message.Chat?.Title, e.Message.Text);

            User member;
            bool userIsRegistered = false;
            using (var db = new ChapubelichdbContext())
            {
                member = db.Users.FirstOrDefault(x => x.UserId == e.Message.From.Id);
                if (member != null)
                {
                    UpdateMemberInfo(e.Message.From, member, db);
                    userIsRegistered = true;
                }
            }

            switch (e.Message.Chat.Type)
            {
                case Telegram.Bot.Types.Enums.ChatType.Private:
                    PrivateMessageProcessAsync(e, userIsRegistered);
                    break;
                case Telegram.Bot.Types.Enums.ChatType.Group:
                    GroupMessageProcessAsync(e, userIsRegistered);
                    break;
                case Telegram.Bot.Types.Enums.ChatType.Supergroup:
                    GroupMessageProcessAsync(e, userIsRegistered);
                    break;
            }
        }
        static void CallbackProcess(object sender, CallbackQueryEventArgs e)
        {
            AllCallbackProcessAsync(sender, e);
        }
        private static async void PrivateMessageProcessAsync(MessageEventArgs e, bool userIsRegistered)
        {
            bool repeatedRegisterRequest = false;
            if (userIsRegistered)
            {
                foreach (var privateCommand in Bot.BotPrivateCommandsList)
                    if (privateCommand.Contains(e.Message.Text, privateChat: true))
                    {
                        await privateCommand.ExecuteAsync(e.Message, client);
                        return;
                    }

                foreach (var regexCommand in Bot.BotRegexCommandsList)
                    if (regexCommand.Contains(e.Message.Text))
                    {
                        await regexCommand.ExecuteAsync(e.Message, client);
                        return;
                    }
            }

            if (Bot.StartCommand.Contains(e.Message.Text, privateChat: true))
            {
                if (!userIsRegistered)
                {
                    await Bot.StartCommand.ExecuteAsync(e.Message, client);
                    return;
                }
                repeatedRegisterRequest = true;
            }
            else if (Bot.RegistrationCommand.Contains(e.Message.Text, privateChat: true))
            {
                if (!userIsRegistered)
                {
                    await Bot.RegistrationCommand.ExecuteAsync(e.Message, client);
                    return;
                }
                repeatedRegisterRequest = true;
            }

            if (repeatedRegisterRequest)
            {
                await client.TrySendTextMessageAsync(
                e.Message.Chat.Id,
                $"Вы уже зарегестрированы",
                replyMarkup: ReplyKeyboardsStatic.MainMarkup);

                return;
            }

            if (!userIsRegistered)
                await SendRegistrationAlertAsync(e.Message);

            else await client.TrySendTextMessageAsync(
                e.Message.Chat.Id,
                $"Я вас не понял :С Воспользуйтесь меню. (Если его нет - нажмите на соответствующую кнопку на поле ввода)",
                replyMarkup: ReplyKeyboardsStatic.MainMarkup,
                replyToMessageId: e.Message.MessageId);
        }
        private static async void GroupMessageProcessAsync(MessageEventArgs e, bool userIsRegistered)
        {
            foreach (var groupCommand in Bot.BotGroupCommandsList)
            {
                if (groupCommand.Contains(e.Message.Text, privateChat: false))
                {
                    if (userIsRegistered)
                        await groupCommand.ExecuteAsync(e.Message, client);
                    else
                        await SendRegistrationAlertAsync(e.Message);
                }
            }

            foreach (var regexCommand in Bot.BotRegexCommandsList)
            {
                if (regexCommand.Contains(e.Message.Text))
                    if (userIsRegistered)
                        await regexCommand.ExecuteAsync(e.Message, client);
                    else
                        await SendRegistrationAlertAsync(e.Message);
            }
        }
        private static async void AllCallbackProcessAsync(object sender, CallbackQueryEventArgs e)
        {
            if (e.CallbackQuery.Data == null)
                return;

            User member;
            bool userIsRegistered = false;
            using (var db = new ChapubelichdbContext())
            {
                member = db.Users.FirstOrDefault(x => x.UserId == e.CallbackQuery.From.Id);
                if (member != null)
                {
                    UpdateMemberInfo(e.CallbackQuery.From, member, db);
                    userIsRegistered = true;
                }
            }

            var callbackMessages = Bot.CallBackMessagesList;
            foreach (var command in callbackMessages)
            {
                if (command.Contains(e.CallbackQuery))
                    if (userIsRegistered)
                    {
                        await command.ExecuteAsync(e.CallbackQuery, client);
                        return;
                    }
                    else
                    {
                        await SendRegistrationAlertAsync(e.CallbackQuery);
                        return;
                    }
            }

            if (Bot.GenderCallbackMessage.Contains(e.CallbackQuery))
                await Bot.GenderCallbackMessage.ExecuteAsync(e.CallbackQuery, client);
        }
        

        private static async Task<Message> SendRegistrationAlertAsync(Message message)
        {
            if (message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private)
            {
                Message registrationMessage = await client.TrySendTextMessageAsync(
                        message.Chat.Id,
                        $"Упс, кажется вас нет в базе данных. Пожалуйста, пройдите процесс регистрации: ",
                        replyToMessageId: message.MessageId);
                await Bot.RegistrationCommand.ExecuteAsync(message, client);
                return registrationMessage;
            }
            else if (message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Group ||
                message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Supergroup)
            {
                return await client.TrySendTextMessageAsync(
                        message.Chat.Id,
                        $"Кажется, вы не зарегистрированы. Для регистрации обратитесь к боту в личные сообщения \U0001F48C",
                        replyToMessageId: message.MessageId
                        );
            }
            return null;
        }
        private static async Task SendRegistrationAlertAsync(CallbackQuery callbackQuery)
        {
            await client.TryAnswerCallbackQueryAsync(
                        callbackQuery.Id,
                        $"Пожалуйста, пройдите процесс регистрации.",
                        showAlert: true);
        }
        private static void UpdateMemberInfo(Telegram.Bot.Types.User sender, User member, ChapubelichdbContext db)
        {
            if (member.FirstName != sender.FirstName)
            {
                member.FirstName = sender.FirstName;
                db.SaveChanges();
            }
        }
    }
}
