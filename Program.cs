using Chapubelich.ChapubelichBot.Init;
using Chapubelich.Chating.Commands;
using Chapubelich.Database;
using Chapubelich.Extensions;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace Chapubelich
{
    class Program
    {
        private static ITelegramBotClient client = Bot.Client;
        static void Main(string[] args)
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

            switch (e.Message.Chat.Type)
            {
                case Telegram.Bot.Types.Enums.ChatType.Private:
                    PrivateMessageProcess(e);
                    break;
                case Telegram.Bot.Types.Enums.ChatType.Group:
                    GroupMessageProcess(e);
                    break;
                case Telegram.Bot.Types.Enums.ChatType.Supergroup:
                    GroupMessageProcess(e);
                    break;
            }
        }
        static async void PrivateMessageProcess(MessageEventArgs e)
        {
            if (Registred(e.Message.From))
            {
                foreach (var privateCommand in Bot.BotPrivateCommandsList)
                {
                    if (privateCommand.Contains(e.Message.Text, privateChat: true))
                        privateCommand.Execute(e.Message, client);
                }

                foreach (var regexCommand in Bot.BotRegexCommandsList)
                {
                    if (regexCommand.Contains(e.Message.Text))
                        regexCommand.Execute(e.Message, client);
                }
            }
            else
            {
                var startCommand = Bot.BotPrivateCommandsList.First(x => x.Name == "/start");
                var registrationCommand = Bot.BotPrivateCommandsList.First(x => x.Name == "\U0001F511 Register");

                if (!startCommand.Contains(e.Message.Text, privateChat: true) && !registrationCommand.Contains(e.Message.Text, privateChat: true))
                    await client.TrySendTextMessageAsync(
                    e.Message.Chat.Id,
                    $"Упс, кажется вас нет в базе данных. Пожалуйста, пройдите процесс регистрации: ");

                registrationCommand.Execute(e.Message, client);
            }
        }
        static async void GroupMessageProcess(MessageEventArgs e)
        {
            bool registred = Registred(e.Message.From);

            foreach (var groupCommand in Bot.BotGroupCommandsList)
            {
                if (groupCommand.Contains(e.Message.Text, privateChat: false))
                {
                    if (registred)
                        groupCommand.Execute(e.Message, client);
                    else 
                        await client.TrySendTextMessageAsync(
                        e.Message.Chat.Id,
                        $"Кажется, вы не зарегистрированы. Для регистрации обратитесь к боту в личные сообщения \U0001F48C",
                        replyToMessageId: e.Message.MessageId);
                }
            }

            foreach (var regexCommand in Bot.BotRegexCommandsList)
            {
                if (regexCommand.Contains(e.Message.Text))
                    if (registred)
                        regexCommand.Execute(e.Message, client);
                    else 
                        await client.TrySendTextMessageAsync(
                        e.Message.Chat.Id,
                        $"Кажется, вы не зарегистрированы. Для регистрации обратитесь к боту в личные сообщения \U0001F48C",
                        replyToMessageId: e.Message.MessageId
                        );
            }
        }

        static void OnCallBack(object sender, CallbackQueryEventArgs e)
        {
            if (e.CallbackQuery.Data == null)
                return;

            var callbackMessages = Bot.CallBackMessagesList;
            foreach (var command in callbackMessages)
            {
                if (command.Contains(e.CallbackQuery))
                    command.Execute(e.CallbackQuery, client);
            }
        }

        static bool Registred(Telegram.Bot.Types.User sender)
        {
            using (var db = new ChapubelichdbContext())
                return db.Users.Any(x => x.UserId == sender.Id);
        }
    }
}
