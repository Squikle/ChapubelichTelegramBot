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
        private static readonly IConfiguration Config = Bot.GetConfig();
        private static readonly int BotId = Client.GetMeAsync().Result.Id;
        public static void Start()
        {
            RouletteGameManager.Init(Client);
            RestoreData();
            DailyProcess();
            Client.StartReceiving();
            Client.OnMessage += MessageProcessAsync;
            Client.OnCallbackQuery += CallbackProcess;
            Console.WriteLine("StartReceiving...");
        }
        public static void Stop()
        {
            RouletteGameManager.Terminate();
            Client.OnMessage -= MessageProcessAsync;
            Client.OnCallbackQuery -= CallbackProcess;
            if (Client.IsReceiving)
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
            List<RouletteGameSession> gameSessionsToResume;
            await using var db = new ChapubelichdbContext();
            {
                gameSessionsToResume =
                   db.RouletteGameSessions
                       .Include(gs => gs.ColorBetTokens)
                       .ThenInclude(bt => bt.User)
                       .Include(gs => gs.NumberBetTokens)
                       .ThenInclude(bt => bt.User)
                       .Where(gs => gs.Resulting)
                       .ToList();
            }

            Parallel.ForEach(gameSessionsToResume,
                async gs => await RouletteGameManager.ResumeResultingAsync(gs.ChatId));
        }

        private static async void MessageProcessAsync(object sender, MessageEventArgs e)
        {
            if (e.Message.ForwardFrom != null)
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

            if (await AdminMessageProcessAsync(e.Message))
                return;

            if (e.Message.Type == MessageType.Text)
            {
                if (groupOfMessage != null)
                {
                    bool userIsRegistered = IsMemberRegistered(groupOfMessage, e.Message.From);
                    await GroupMessageProcessAsync(e.Message, userIsRegistered);
                }
                else
                {
                    bool userIsRegistered = IsUserRegistered(e.Message.From);
                    await PrivateMessageProcessAsync(e.Message, userIsRegistered);
                }
            }
        }
        private static async void CallbackProcess(object sender, CallbackQueryEventArgs e)
        {
            if (e.CallbackQuery.Data == null)
                return;

            bool userIsRegistered;
            if (e.CallbackQuery.Message.Chat.Type == ChatType.Supergroup ||
                e.CallbackQuery.Message.Chat.Type == ChatType.Group)
            {
                Group groupOfMessage = await UpdateGroup(e.CallbackQuery.Message);
                if (groupOfMessage != null && !groupOfMessage.IsAvailable)
                    return;

                userIsRegistered = IsMemberRegistered(groupOfMessage, e.CallbackQuery.From);
            }
            else
                userIsRegistered = IsUserRegistered(e.CallbackQuery.From);

            var callbackMessages = Bot.CallBackMessagesList;
            foreach (var command in callbackMessages)
            {
                if (command.Contains(e.CallbackQuery))
                    if (userIsRegistered)
                    {
                        await command.ExecuteAsync(e.CallbackQuery, Client);
                        return;
                    }
                    else
                    {
                        await SendRegistrationAlertAsync(e.CallbackQuery);
                        return;
                    }
            }
            if (Bot.GenderCallbackMessage.Contains(e.CallbackQuery))
                await Bot.GenderCallbackMessage.ExecuteAsync(e.CallbackQuery, Client);
        }
        private static async Task<bool> AdminMessageProcessAsync(Message message)
        {
            if (message.From.Id == 243857110)
            {
                foreach (var adminRegexCommand in Bot.BotAdminRegexCommandsList)
                {
                    if (message.Text != null && adminRegexCommand.Contains(message.Text)
                        || message.Caption != null && adminRegexCommand.Contains(message.Caption))
                    {
                        await adminRegexCommand.ExecuteAsync(message, Client);
                        return true;
                    }
                }

                if (message.Type == MessageType.Text)
                {
                    foreach (var adminCommand in Bot.BotAdminCommandsList)
                    {
                        if (adminCommand.Contains(message.Text, privateChat: true))
                        {
                            await adminCommand.ExecuteAsync(message, Client);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static async Task<bool> PrivateMessageProcessAsync(Message message, bool isUserRegistered)
        {
            if (message.Type != MessageType.Text)
                return false;

            bool repeatedRegisterRequest = false;
            if (isUserRegistered)
            {
                foreach (var privateCommand in Bot.BotPrivateCommandsList)
                    if (privateCommand.Contains(message.Text, privateChat: true))
                    {
                        await privateCommand.ExecuteAsync(message, Client);
                        return true;
                    }

                foreach (var regexCommand in Bot.BotRegexCommandsList)
                    if (regexCommand.Contains(message.Text))
                    {
                        await regexCommand.ExecuteAsync(message, Client);
                        return true;
                    }
            }

            if (Bot.StartCommand.Contains(message.Text, privateChat: true))
            {
                if (!isUserRegistered)
                {
                    await Bot.StartCommand.ExecuteAsync(message, Client);
                    return true;
                }

                repeatedRegisterRequest = true;
            }
            else if (Bot.RegistrationCommand.Contains(message.Text, privateChat: true))
            {
                if (!isUserRegistered)
                {
                    await Bot.RegistrationCommand.ExecuteAsync(message, Client);
                    return true;
                }

                repeatedRegisterRequest = true;
            }

            if (repeatedRegisterRequest)
            {
                await Client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Ты уже зарегестрирован👍",
                    replyMarkup: ReplyKeyboards.MainMarkup);

                return true;
            }

            if (!isUserRegistered)
            {
                await SendRegistrationAlertAsync(message);
                return true;
            }

            await Client.TrySendTextMessageAsync(
                message.Chat.Id,
                "Я тебя не понял :С Воспользуйся меню. (Если его нет - нажми на соответствующую кнопку на поле ввода👇)",
                replyMarkup: ReplyKeyboards.MainMarkup,
                replyToMessageId: message.MessageId);
            return true;
        }
        private static async Task<bool> GroupMessageProcessAsync(Message message, bool isUserRegistered)
        {
            if (message.Type != MessageType.Text)
                return false;

            foreach (var groupCommand in Bot.BotGroupRegexCommandsList)
            {
                if (groupCommand.Contains(message.Text))
                {
                    if (isUserRegistered)
                        await groupCommand.ExecuteAsync(message, Client);
                    else
                        await SendRegistrationAlertAsync(message);

                    return true;
                }
            }

            foreach (var regexCommand in Bot.BotRegexCommandsList)
            {
                if (regexCommand.Contains(message.Text))
                {
                    if (isUserRegistered)
                        await regexCommand.ExecuteAsync(message, Client);
                    else
                        await SendRegistrationAlertAsync(message);

                    return true;
                } 
            }

            return false;
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

            if (group.Name != message.Chat.Title)
            {
                group.Name = message.Chat.Title;
                saveChangesRequired = true;
            }

            var botAsChatMember = await gettingChatMember;

            bool isChatAvailableToSend = false;
            if (botAsChatMember != null)
                isChatAvailableToSend = (botAsChatMember.CanSendMessages ?? true)
                                        && (botAsChatMember.CanSendMediaMessages ?? true)
                                        && (botAsChatMember.IsMember ?? true);

            if (group.IsAvailable != isChatAvailableToSend)
            {
                group.IsAvailable = isChatAvailableToSend;
                saveChangesRequired = true;
            }

            if (message.Type == MessageType.ChatMemberLeft)
            {
                Database.Models.User leftUser = group.Users.FirstOrDefault(x => x.UserId == message.LeftChatMember.Id);
                if (leftUser != null && group.Users.Contains(leftUser))
                {
                    group.Users.Remove(leftUser);
                    saveChangesRequired = true;
                }
            }
            else
            {
                var senderUser = db.Users.FirstOrDefault(u => u.UserId == message.From.Id);
                if (senderUser != null && group.Users.All(u => u.UserId != senderUser.UserId))
                {
                    group.Users.Add(senderUser);
                    saveChangesRequired = true;
                }
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
