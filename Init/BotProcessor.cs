using System;
using System.Collections.Generic;
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
using Telegram.Bot.Types.Enums;
using User = Telegram.Bot.Types.User;

namespace ChapubelichBot.Init
{
    static class BotProcessor
    {
        private static readonly ITelegramBotClient Client = Bot.GetClient();
        private static readonly IConfiguration     Config = Bot.GetConfig();
        private static readonly int BotId = Client.GetMeAsync().Result.Id;
        public static void Start()
        {
            RestoreData();
            DailyProcess();
            Client.StartReceiving();
            Client.OnMessage += MessageProcessAsync;
            Client.OnCallbackQuery += CallbackProcess;
            Console.WriteLine("StartReceiving...");
        }
        public static void Stop()
        {
            Client.OnMessage -= MessageProcessAsync;
            Client.OnCallbackQuery -= CallbackProcess;
            Client.StopReceiving();
            Console.WriteLine("StopReceiving...");
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

            Task dailyResetTask = null;
            //reset data if not done before
            bool alreadyRestarted = false;
            await using (var db = new ChapubelichdbContext())
            {
                if (db.Configurations.First().LastResetTime > new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 00, 00, 00))
                    alreadyRestarted = true;
            }
            if (!alreadyRestarted)
                dailyResetTask = DailyResetJob.ExecuteManually();

            //send compliments if not done before
            var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 00, 00);
            if (DateTime.Now > date)
                await DailyComplimentJob.ExecuteManually(Client);

            if (dailyResetTask != null)
                await dailyResetTask;
        }

        private static async void RestoreData()
        {
            List<Task> resumingTasks = new List<Task>();
            await using var db = new ChapubelichdbContext();
            foreach (var gameSessionData in db.RouletteGameSessions)
            {
                var gameSession = RouletteGameSessionBuilder.Create().RestoreFrom(gameSessionData, Client).AddToSessionsList()
                    .Build();
                if (gameSession.Resulting)
                    resumingTasks.Add(gameSession.ResumeResultingAsync(Client));
            }

            Task.WaitAll(resumingTasks.ToArray());
        }

        private static async void MessageProcessAsync(object sender, MessageEventArgs e)
        {
            if (e.Message?.Text == null || e.Message.ForwardFrom != null)
                return;

            Group groupOfMessage = null;
            if (e.Message.Chat.Type == ChatType.Supergroup || e.Message.Chat.Type == ChatType.Group)
            {
                groupOfMessage = await UpdateGroup(e.Message);
                if (groupOfMessage != null && !groupOfMessage.IsAvailable)
                    return;
            }

            Console.WriteLine("{0:HH:mm:ss}: {1} {2}| {3} ({4} | {5}): [{6}] {7}", e.Message.Date, e.Message.Type,
                e.Message.From.Id, e.Message.From.Username,
                e.Message.Chat.Id, e.Message.Chat?.Title, e.Message.MessageId, e.Message.Text);

            if (e.Message.Date.AddMinutes(Config.GetValue<int>("AppSettings:MessageCheckPeriod")) < DateTime.UtcNow)
                return;

            if (e.Message.From.Id == 243857110)
            {
                foreach (var adminRegexCommand in Bot.BotAdminRegexCommandsList)
                    if (e.Message.Text != null && adminRegexCommand.Contains(e.Message.Text)
                        || e.Message.Caption != null && adminRegexCommand.Contains(e.Message.Caption))
                    {
                        await adminRegexCommand.ExecuteAsync(e.Message, Client);
                        return;
                    }

                foreach (var adminCommand in Bot.BotAdminCommandsList)
                {
                    if (adminCommand.Contains(e.Message.Text, privateChat: true))
                    {
                        await adminCommand.ExecuteAsync(e.Message, Client);
                        return;
                    }
                }
            }

            if (groupOfMessage != null)
            {
                bool userIsRegistered = IsMemberRegistered(groupOfMessage, e.Message.From);
                GroupMessageProcessAsync(e.Message, userIsRegistered);
            }
            else
            {
                bool userIsRegistered = IsUserRegistered(e.Message.From);
                PrivateMessageProcessAsync(e.Message, userIsRegistered);
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
            {
                await SendRegistrationAlertAsync(message);
            }
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

            bool userIsRegistered;
            if (callbackQuery.Message.Chat.Type == ChatType.Supergroup ||
                callbackQuery.Message.Chat.Type == ChatType.Group)
            {
                Group groupOfMessage = await UpdateGroup(callbackQuery.Message);
                if (groupOfMessage != null && !groupOfMessage.IsAvailable)
                    return;

                userIsRegistered = IsMemberRegistered(groupOfMessage, callbackQuery.From);
            }
            else
                userIsRegistered = IsUserRegistered(callbackQuery.From);

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

        private static async Task<Group> UpdateGroup(Message message)
        {
            Task<ChatMember> gettingChatMember = Client.GetChatMemberAsync(message.Chat.Id, BotId);

            bool saveChangesRequired = false;

            await using var db = new ChapubelichdbContext();
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
                saveChangesRequired = true;
            }

            var botMember = await gettingChatMember;

            bool isChatAvailableToSend = false;
            if (botMember != null)
                isChatAvailableToSend = (botMember.CanSendMessages ?? true)
                                        && (botMember.CanSendMediaMessages ?? true)
                                        && (botMember.IsMember ?? true);

            if (group.IsAvailable != isChatAvailableToSend)
            {
                group.IsAvailable = isChatAvailableToSend;
                saveChangesRequired = true;
            }
            
            var user = db.Users.FirstOrDefault(u => u.UserId == message.From.Id);
            if (user != null && group.Users.All(u => u.UserId != user.UserId))
            {
                group.Users.Add(user);
                saveChangesRequired = true;
            }

            if (saveChangesRequired)
                db.SaveChanges();

            return group;
        }
        private static bool IsUserRegistered(User user)
        {
            using var db = new ChapubelichdbContext();
            return db.Users.Any(x => x.UserId == user.Id);
        }
        private static bool IsMemberRegistered(Group group, User user)
        {
            return group.Users.Any(x => x.UserId == user.Id);
        }

        private static async Task SendRegistrationAlertAsync(Message message)
        {
            if (message.Chat.Type == ChatType.Private)
            {
                await Client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Упс, кажется тебя нет в базе данных. Пожалуйста, пройди процесс регистрации: ",
                    replyToMessageId: message.MessageId);
                await Bot.RegistrationCommand.ExecuteAsync(message, Client);
            }
            else if (message.Chat.Type == ChatType.Group ||
                message.Chat.Type == ChatType.Supergroup)
            {
                await Client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Кажется, вы не зарегистрированы. Для регистрации обратитесь к боту в личные сообщения 💌",
                    replyToMessageId: message.MessageId
                );
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
