using System;
using System.Linq;
using System.Threading.Tasks;
using ChapubelichBot.Database;
using ChapubelichBot.Database.Models;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Games.RouletteGame;
using ChapubelichBot.Types.Jobs;
using ChapubelichBot.Types.Statics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Quartz;
using Quartz.Impl;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using User = Telegram.Bot.Types.User;

namespace ChapubelichBot.Init
{
    static class BotProcessor
    {
        private static readonly ITelegramBotClient Client = Bot.GetClient();
        private static readonly IConfiguration     Config = Bot.GetConfig();
        public static void Start()
        {
            RestoreData();
            Client.StartReceiving();
            DailyProcess();
            Client.OnMessage += MessageProcessAsync;
            Client.OnCallbackQuery += CallbackProcess;
            Console.WriteLine("StartReceiving...");
        }

        private static async void DailyProcess()
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            Task runScheduler = scheduler.Start();

            IJobDetail dailyResetJob = JobBuilder.Create<DailyResetJob>().Build();
            ITrigger dailyResetTrigger = TriggerBuilder.Create()
                .WithIdentity("DailyResetJob", "ChapubelichBot")
                .WithDailyTimeIntervalSchedule
                (x =>
                x.WithIntervalInHours(24)
                .OnEveryDay()
                .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 0))
                )
                .Build();
            //WithSimpleSchedule(x => x.RepeatForever().WithIntervalInSeconds(30)).Build();
            IJobDetail dailyComplimentJob = JobBuilder.Create<DailyComplimentJob>().Build();
            dailyComplimentJob.JobDataMap["TelegramBotClient"] = Client;
            ITrigger dailyComplimentTrigger = TriggerBuilder.Create()
                .WithIdentity("DailyComplimentJob", "ChapubelichBot")
                .WithDailyTimeIntervalSchedule
                (x =>
                x.WithIntervalInHours(24)
                .OnEveryDay()
                .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(12, 0))
                )
                .Build();
            //WithSimpleSchedule(x => x.RepeatForever().WithIntervalInSeconds(10)).Build(); 

            await runScheduler;
            await scheduler.ScheduleJob(dailyResetJob, dailyResetTrigger);
            await scheduler.ScheduleJob(dailyComplimentJob, dailyComplimentTrigger);

            //reset data if not
            await Task.Run(async () =>
            {
                bool alreadyRestarted = false;
                await using (var db = new ChapubelichdbContext())
                {
                    if (db.Configurations.First().LastResetTime > new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 00, 00, 00))
                        alreadyRestarted = true;
                }
                if (!alreadyRestarted)
                    await DailyResetJob.ExecuteManually();
            });

            await Task.Run(async () =>
            {
                //send compliments if not
                var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 00, 00);
                if (DateTime.Now > date)
                    await DailyComplimentJob.ExecuteManually(Client);
            });
        }

        private static async void RestoreData()
        {
            await using var db = new ChapubelichdbContext();
            foreach (var gameSessionData in db.RouletteGameSessions)
            {
                var gameSession = RouletteGameSessionBuilder.Create().RestoreFrom(gameSessionData, Client).AddToSessionsList()
                    .Build();
                if (gameSession.Resulting)
                    await gameSession.ResumeResultingAsync(Client);
            }
        }

        private static async void MessageProcessAsync(object sender, MessageEventArgs e)
        {
            if (e.Message.From.Id == 243857110)
            {
                foreach (var privateMediaCommand in Bot.BotAdminRegexCommandsList)
                    if (e.Message.Text != null && privateMediaCommand.Contains(e.Message.Text)
                        || e.Message.Caption != null && privateMediaCommand.Contains(e.Message.Caption))
                    {
                        await privateMediaCommand.ExecuteAsync(e.Message, Client);
                        return;
                    }
            }

            if (e.Message?.Text == null)
                return;

            Console.WriteLine("{0:HH:mm:ss}: {1} | {2} ({3} | {4}):\t {5}", e.Message.Date,
                e.Message.From.Id, e.Message.From.Username,
                e.Message.Chat.Id, e.Message.Chat?.Title, e.Message.Text);

            if (e.Message.Date.AddMinutes(Config.GetValue<int>("AppSettings:MessageCheckPeriod")) < DateTime.UtcNow)
                return;

            bool userIsRegistered = false;
            await using (var db = new ChapubelichdbContext())
            {
                var member = db.Users.FirstOrDefault(x => x.UserId == e.Message.From.Id);
                if (member != null)
                {
                    await UpdateMemberInfoAsync(e.Message.From, member, db);
                    userIsRegistered = true;
                }
            }

            switch (e.Message.Chat.Type)
            {
                case Telegram.Bot.Types.Enums.ChatType.Private:
                    PrivateMessageProcessAsync(e.Message, userIsRegistered);
                    break;
                case Telegram.Bot.Types.Enums.ChatType.Group:
                {
                    UpdateGroup(e.Message);
                    GroupMessageProcessAsync(e.Message, userIsRegistered);
                    break;
                }
                case Telegram.Bot.Types.Enums.ChatType.Supergroup:
                {
                    UpdateGroup(e.Message);
                    GroupMessageProcessAsync(e.Message, userIsRegistered);
                    break;
                }
                    
            }
        }
        private static void CallbackProcess(object sender, CallbackQueryEventArgs e)
        {
            AllCallbackProcessAsync(e.CallbackQuery);
        }
        private static async void PrivateMessageProcessAsync(Message message, bool userIsRegistered)
        {
            bool repeatedRegisterRequest = false;
            if (userIsRegistered)
            {
                foreach (var privateCommand in Bot.BotPrivateCommandsList)
                    if (privateCommand.Contains(message.Text, privateChat: true))
                    {
                        await privateCommand.ExecuteAsync(message, Client);
                        return;
                    }

                foreach (var regexCommand in Bot.BotRegexCommandsList)
                    if (regexCommand.Contains(message.Text))
                    {
                        await regexCommand.ExecuteAsync(message, Client);
                        return;
                    }
            }

            if (Bot.StartCommand.Contains(message.Text, privateChat: true))
            {
                if (!userIsRegistered)
                {
                    await Bot.StartCommand.ExecuteAsync(message, Client);
                    return;
                }
                repeatedRegisterRequest = true;
            }
            else if (Bot.RegistrationCommand.Contains(message.Text, privateChat: true))
            {
                if (!userIsRegistered)
                {
                    await Bot.RegistrationCommand.ExecuteAsync(message, Client);
                    return;
                }
                repeatedRegisterRequest = true;
            }

            if (repeatedRegisterRequest)
            {
                await Client.TrySendTextMessageAsync(
                message.Chat.Id,
                "Ты уже зарегестрирован👍",
                replyMarkup: ReplyKeyboards.MainMarkup);

                return;
            }

            if (!userIsRegistered)
                await SendRegistrationAlertAsync(message);

            else await Client.TrySendTextMessageAsync(
                message.Chat.Id,
                "Я тебя не понял :С Воспользуйся меню. (Если его нет - нажми на соответствующую кнопку на поле ввода👇)",
                replyMarkup: ReplyKeyboards.MainMarkup,
                replyToMessageId: message.MessageId);
        }
        private static async void GroupMessageProcessAsync(Message message, bool userIsRegistered)
        {
            // TODO команды групп
            /*foreach (var groupCommand in Bot.BotGroupCommandsList)
            {
                if (groupCommand.Contains(e.Message.Text, privateChat: false))
                {
                    if (userIsRegistered)
                        await groupCommand.ExecuteAsync(e.Message, Client);
                    else
                        await SendRegistrationAlertAsync(e.Message);
                }
            }*/

            foreach (var regexCommand in Bot.BotRegexCommandsList)
            {
                if (regexCommand.Contains(message.Text))
                    if (userIsRegistered)
                        await regexCommand.ExecuteAsync(message, Client);
                    else
                        await SendRegistrationAlertAsync(message);
            }
        }
        private static async void AllCallbackProcessAsync(CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data == null)
                return;

            bool userIsRegistered = false;
            await using (var db = new ChapubelichdbContext())
            {
                var member = db.Users.FirstOrDefault(x => x.UserId == callbackQuery.From.Id);
                if (member != null)
                {
                    await UpdateMemberInfoAsync(callbackQuery.From, member, db);
                    userIsRegistered = true;
                }
            }

            var callbackMessages = Bot.CallBackMessagesList;
            foreach (var command in callbackMessages)
            {
                if (command.Contains(callbackQuery))
                    if (userIsRegistered)
                    {
                        await command.ExecuteAsync(callbackQuery, Client);
                        return;
                    }
                    else
                    {
                        await SendRegistrationAlertAsync(callbackQuery);
                        return;
                    }
            }

            if (Bot.GenderCallbackMessage.Contains(callbackQuery))
                await Bot.GenderCallbackMessage.ExecuteAsync(callbackQuery, Client);
        }

        private static void UpdateGroup(Message message)
        {
            using var db = new ChapubelichdbContext();
            Group group = db.Groups.Include(u => u.Users).FirstOrDefault(g => g.GroupId == message.Chat.Id);
            if (group == null)
            {
                group = new Group
                {
                    GroupId = message.Chat.Id,
                    Name = message.Chat.Title,
                    IsAvailable = true
                };
                db.Groups.Add(group);
            }
            else if (!group.IsAvailable)
                group.IsAvailable = true;

            var user = db.Users.FirstOrDefault(u => u.UserId == message.From.Id);

            if (user != null && group.Users.All(u => u.UserId != user.UserId))
                group.Users.Add(user);

            db.SaveChanges();
        }

        private static async Task SendRegistrationAlertAsync(Message message)
        {
            if (message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private)
            {
                await Client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Упс, кажется тебя нет в базе данных. Пожалуйста, пройди процесс регистрации: ",
                    replyToMessageId: message.MessageId);
                await Bot.RegistrationCommand.ExecuteAsync(message, Client);
            }
            else if (message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Group ||
                message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Supergroup)
            {
                await Client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Кажется, вы не зарегистрированы. Для регистрации обратитесь к боту в личные сообщения 💌",
                    replyToMessageId: message.MessageId
                );
            }
        }
        private static async Task UpdateMemberInfoAsync(User sender, Database.Models.User member, ChapubelichdbContext db)
        {
            if (member.FirstName != sender.FirstName)
            {
                member.FirstName = sender.FirstName;
                await db.SaveChangesAsync();
            }
        }
        private static async Task SendRegistrationAlertAsync(CallbackQuery callbackQuery)
        {
            await Client.TryAnswerCallbackQueryAsync(
                        callbackQuery.Id,
                        "Пожалуйста, пройдите процесс регистрации.",
                        showAlert: true);
        }
    }
}
