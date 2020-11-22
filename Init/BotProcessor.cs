using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChapubelichBot.Database;
using ChapubelichBot.Database.Models;
using ChapubelichBot.Types.Games.RouletteGame;
using ChapubelichBot.Types.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Quartz;
using Quartz.Impl;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace ChapubelichBot.Init
{
    static class BotProcessor
    {
        private static readonly ITelegramBotClient Client = Bot.GetClient();
        private static readonly IConfiguration Config = Bot.GetConfig();
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
            Console.WriteLine("{0:HH:mm:ss}: {1} {2}| {3} ({4} | {5}): [{6}] {7}", e.Message.Date, e.Message.Type,
                e.Message.From.Id, e.Message.From.Username,
                e.Message.Chat.Id, e.Message.Chat?.Title, e.Message.MessageId, e.Message.Text);

            if (e.Message.Date.AddMinutes(Config.GetValue<int>("AppSettings:MessageCheckPeriod")) < DateTime.UtcNow)
                return;

            foreach (var messageProcessor in Bot.BotMessageProcessorsList)
            {
                if (await Bot.AdminMessageProcessor.Execute(e.Message, Client))
                    return;
                if (await messageProcessor.Execute(e.Message, Client))
                    return;
            }
        }
        private static async void CallbackProcess(object sender, CallbackQueryEventArgs e)
        {
            foreach (var messageProcessor in Bot.BotCallbackMessageProcessorsList)
            {
                if (await messageProcessor.Execute(e.CallbackQuery, Client))
                    return;
            }
        }
    }
}
