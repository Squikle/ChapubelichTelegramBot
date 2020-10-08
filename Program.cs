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

namespace ChapubelichBot
{
    class Program
    {
        private static readonly ITelegramBotClient client = Bot.Client;
        static void Main()
        {
            var me = client.GetMeAsync().Result;
            Console.Title = me.Username;

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

                db.SaveChanges();*//*
                var li1 = db.Set<UserGroup>().Where(x => x.User.Username == "Squikle").Select(x => x.Group.Name);
                var li2 = db.Set<UserGroup>().Where(x => x.Group.Name == "Only me").Select(x => x.User.Username);

                foreach (var el in li2)
                    Console.WriteLine(el);
            }*/
            client.OnMessage += MessageProcess;
            client.OnCallbackQuery += OnCallBack;

            Thread.Sleep(int.MaxValue);
        }


        static void MessageProcess(object sender, MessageEventArgs e)
        {
            if (null == e.Message || null == e.Message.Text)
                return;

            var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            if (e.Message.Date.AddHours(offset.Hours).AddMinutes(AppSettings.MessagesPeriod) > DateTime.Now)
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
                    PrivateMessageProcess(e, userIsRegistered);
                    break;
                case Telegram.Bot.Types.Enums.ChatType.Group:
                    GroupMessageProcess(e, userIsRegistered);
                    break;
                case Telegram.Bot.Types.Enums.ChatType.Supergroup:
                    GroupMessageProcess(e, userIsRegistered);
                    break;
            }
        }
        private static async void PrivateMessageProcess(MessageEventArgs e, bool userIsRegistered)
        {
            bool repeatedRegisterRequest = false;

            if (userIsRegistered)
            {
                foreach (var privateCommand in Bot.BotPrivateCommandsList)
                    if (privateCommand.Contains(e.Message.Text, privateChat: true))
                    {
                        privateCommand.Execute(e.Message, client);
                        return;
                    }

                foreach (var regexCommand in Bot.BotRegexCommandsList)
                    if (regexCommand.Contains(e.Message.Text))
                    {
                        regexCommand.Execute(e.Message, client);
                        return;
                    }
            }

            if (Bot.StartCommand.Contains(e.Message.Text, privateChat: true))
            {
                if (!userIsRegistered)
                {
                    Bot.StartCommand.Execute(e.Message, client);
                    return;
                }
                repeatedRegisterRequest = true;
            }
            else if (Bot.RegistrationCommand.Contains(e.Message.Text, privateChat: true))
            {
                if (!userIsRegistered)
                {
                    Bot.RegistrationCommand.Execute(e.Message, client);
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
                SendRegistrationAlert(e.Message);

            else await client.TrySendTextMessageAsync(
                e.Message.Chat.Id,
                $"Воспользуйтесь меню. (Если его нет - нажмите на соответствующую кнопку на поле ввода)",
                replyMarkup: ReplyKeyboardsStatic.MainMarkup,
                replyToMessageId: e.Message.MessageId);
        }
        private static void GroupMessageProcess(MessageEventArgs e, bool userIsRegistered)
        {
            foreach (var groupCommand in Bot.BotGroupCommandsList)
            {
                if (groupCommand.Contains(e.Message.Text, privateChat: false))
                {
                    if (userIsRegistered)
                        groupCommand.Execute(e.Message, client);
                    else
                        SendRegistrationAlert(e.Message);
                }
            }

            foreach (var regexCommand in Bot.BotRegexCommandsList)
            {
                if (regexCommand.Contains(e.Message.Text))
                    if (userIsRegistered)
                        regexCommand.Execute(e.Message, client);
                    else
                        SendRegistrationAlert(e.Message);
            }
        }

        private static void OnCallBack(object sender, CallbackQueryEventArgs e)
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
                        command.Execute(e.CallbackQuery, client);
                    else
                        SendRegistrationAlert(e.CallbackQuery);

                if (Bot.GenderCallbackMessage.Contains(e.CallbackQuery))
                    Bot.GenderCallbackMessage.Execute(e.CallbackQuery, client);
            }
        }
        private static async void SendRegistrationAlert(Message message)
        {
            if (message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private)
            {
                await client.TrySendTextMessageAsync(
                        message.Chat.Id,
                        $"Упс, кажется вас нет в базе данных. Пожалуйста, пройдите процесс регистрации: ",
                        replyToMessageId: message.MessageId);
                Bot.RegistrationCommand.Execute(message, client);
                return;
            }
            else if (message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Group ||
                message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Supergroup)
            {
                await client.TrySendTextMessageAsync(
                        message.Chat.Id,
                        $"Кажется, вы не зарегистрированы. Для регистрации обратитесь к боту в личные сообщения \U0001F48C",
                        replyToMessageId: message.MessageId
                        );
            }
        }
        private static async void SendRegistrationAlert(CallbackQuery callbackQuery)
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
