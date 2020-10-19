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
using System.Threading.Tasks;
using ChapubelichBot.Database.Models;
using ChapubelichBot.Types.Abstractions;
using System.Collections.Generic;
using System.Data.Entity;
using Quartz;

namespace ChapubelichBot
{
    class Program
    {
        private static readonly ITelegramBotClient client = Bot.Client;
        private static Timer ComplimentsSender;
        static void Main()
        {
            var me = client.GetMeAsync();
            //Console.Title = me.Username;

            Console.WriteLine($"StartReceiving...");
            client.StartReceiving();

            client.OnMessage += MessageProcessAsync;
            client.OnCallbackQuery += CallbackProcess;

            ITrigger trigger = TriggerBuilder.Create()
            .WithDailyTimeIntervalSchedule
                (s =>
                    s.OnEveryDay()
                    StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 0))
                )
            .Build();

            Task dailyResetTask = Task.Run(async () =>
            {
                Func<Task> action = async () =>
                {
                    using (var db = new ChapubelichdbContext())
                    {
                        foreach (var user in db.Users)
                        {
                            user.Complimented = false;
                            user.DailyRewarded = false;
                        }
                        await db.SaveChangesAsync();
                    }
                };
            
                var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 00, 00, 00);
                TimeSpan ts;
                if (date > DateTime.Now)
                    ts = date - DateTime.Now;
                else
                {
                    date = date.AddDays(1);
                    ts = date - DateTime.Now;
                    await action();
                }
                await Task.Delay(ts).ContinueWith(async (task) => await action());
            });

            Task dailyComplimentsTask = Task.Run(async () => 
            {
                Func<Task> action = async () =>
                {
                    string[] boyCompliments = null;
                    string[] girlCompliments = null;
                    User[] complimentingUsers;

                    using (var db = new ChapubelichdbContext())
                    {
                        var complimentingUsersDatabase = db.Users.Where(x => x.Username == "Squikle" && x.IsAvailable && !x.Complimented);

                        complimentingUsers = complimentingUsersDatabase.ToArray();
                        if (complimentingUsers.Length <= 0)
                            return;

                        foreach (var user in complimentingUsersDatabase)
                        {
                            user.Complimented = true;
                        }
                        await db.SaveChangesAsync();

                        if (complimentingUsers.Any(x => x.Gender == true))
                            boyCompliments = db.BoyCompliments.Select(x => x.ComplimentText).ToArray();
                        if (complimentingUsers.Any(x => x.Gender == false))
                            girlCompliments = db.GirlCompliments.Select(x => x.ComplimentText).ToArray();
                    }

                    Random rand = new Random();
                    Dictionary<User, string> userCompliments = new Dictionary<User, string>();
                    foreach (var user in complimentingUsers)
                    {
                        string compliment;
                        switch (user.Gender)
                        {
                            case true:
                                compliment = boyCompliments[rand.Next(0, boyCompliments.Length)];
                                break;
                            case false:
                                compliment = girlCompliments[rand.Next(0, girlCompliments.Length)];
                                break;
                            default:
                                compliment = "Твои глаза прекрасны";
                                break;
                        }
                        userCompliments.Add(user, compliment);
                    }

                    foreach (var userCompliment in userCompliments)
                    {
                        Console.WriteLine($"{DateTime.Now} отправляю комплимент");
                        await client.TrySendTextMessageAsync(userCompliment.Key.UserId, $"Твой комплимент дня:\n{userCompliment.Value}");
                    }
                };

                var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 00, 00);
                TimeSpan ts;
                if (date > DateTime.Now)
                    ts = date - DateTime.Now;
                else
                {
                    date = date.AddDays(1);
                    ts = date - DateTime.Now;
                    await action();
                }

                await Task.Delay(ts).ContinueWith((task) =>
                {
                    ComplimentsSender = new Timer(async (obj) => await action(), null, 0, TimeSpan.FromHours(24).TotalMilliseconds);
                });
            });
            
            try
            {
                dailyResetTask.Wait();
                dailyComplimentsTask.Wait();
            }
            catch
            {
                throw;
            }

            Console.Read();

            Thread.Sleep(int.MaxValue);
        }


        static async void MessageProcessAsync(object sender, MessageEventArgs e)
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
                    await UpdateMemberInfoAsync(e.Message.From, member, db);
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
                    await UpdateMemberInfoAsync(e.CallbackQuery.From, member, db);
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
        private static async Task UpdateMemberInfoAsync(Telegram.Bot.Types.User sender, User member, ChapubelichdbContext db)
        {
            if (member.FirstName != sender.FirstName)
            { 
                member.FirstName = sender.FirstName;
                await db.SaveChangesAsync();
            }
        }
    }
}
