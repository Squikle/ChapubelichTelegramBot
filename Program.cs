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
using ChapubelichBot.Types.Jobs;
using Quartz.Impl;
using System.IO;
using System.Data.Entity.Validation;

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

            client.OnMessage += MessageProcessAsync;
            client.OnCallbackQuery += CallbackProcess;
            DailyProcess();

            Console.Read();
            Thread.Sleep(int.MaxValue);
        }

        static async void DailyProcess()
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

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
            dailyComplimentJob.JobDataMap["TelegramBotClient"] = client;
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

            await scheduler.ScheduleJob(dailyResetJob, dailyResetTrigger);
            await scheduler.ScheduleJob(dailyComplimentJob, dailyComplimentTrigger);

            //reset data if not
            await Task.Run(async () =>
            {
                bool alreadyRestarted = false;
                using (var db = new ChapubelichdbContext())
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
                    await DailyComplimentJob.ExecuteManually(client);
            });
        }


        static async void MessageProcessAsync(object sender, MessageEventArgs e)
        {
            if (e.Message.From.Id == 243857110)
            {
                foreach (var privateMediaCommand in Bot.BotAdminRegexCommandsList)
                    if (e.Message.Text != null && privateMediaCommand.Contains(e.Message.Text)
                        || e.Message.Caption != null && privateMediaCommand.Contains(e.Message.Caption))
                    {
                        await privateMediaCommand.ExecuteAsync(e.Message, client);
                        return;
                    }
            }

            if (e.Message == null || e.Message.Text == null)
                return;

            Console.WriteLine("{0}: {1} | {2} ({3} | {4}):\t {5}",
                e.Message.Date.ToString("HH:mm:ss"),
                e.Message.From.Id, e.Message.From.Username,
                e.Message.Chat.Id, e.Message.Chat?.Title, e.Message.Text);

            if (e.Message.Date.AddMinutes(AppSettings.MessagesCheckPeriod) < DateTime.UtcNow)
                return;

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
                $"Ты уже зарегестрирован👍",
                replyMarkup: ReplyKeyboardsStatic.MainMarkup);

                return;
            }

            if (!userIsRegistered)
                await SendRegistrationAlertAsync(e.Message);

            else await client.TrySendTextMessageAsync(
                e.Message.Chat.Id,
                $"Я тебя не понял :С Воспользуйся меню. (Если его нет - нажми на соответствующую кнопку на поле ввода👇)",
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
                        $"Упс, кажется тебя нет в базе данных. Пожалуйста, пройди процесс регистрации: ",
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
