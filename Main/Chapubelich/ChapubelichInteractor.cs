using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChapubelichBot.Types.Entities;
using ChapubelichBot.Types.Entities.Roulette;
using ChapubelichBot.Types.Managers;
using ChapubelichBot.Types.Managers.MessagesSender;
using ChapubelichBot.Types.Managers.MessagesSender.Limiters;
using ChapubelichBot.Types.ScheduledJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Quartz;
using Quartz.Impl;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace ChapubelichBot.Main.Chapubelich
{
    static class ChapubelichInteractor
    {
        private static readonly ITelegramBotClient Client = ChapubelichClient.GetClient();
        private static readonly IConfiguration Config = ChapubelichClient.GetConfig();
        public static async Task StartAsync()
        {
            MessageSenderManager.Init(new GlobalLimiter(30, 1), new ChatLimiter(30, 2));
            RouletteGameManager.Init();
            CrocodileGameManager.Init();
            await RestoreDataAsync();
            await DailyProcessAsync();
            Client.StartReceiving();
            Client.OnMessage += MessageProcessAsync;
            Client.OnCallbackQuery += CallbackProcess;
            Console.WriteLine("StartReceiving...");
        }
        public static void Stop()
        {
            MessageSenderManager.Terminate();
            RouletteGameManager.Terminate();
            CrocodileGameManager.Terminate();
            Client.OnMessage -= MessageProcessAsync;
            Client.OnCallbackQuery -= CallbackProcess;
            if (Client.IsReceiving)
                Client.StopReceiving();
            Console.WriteLine("StopReceiving...");
        }

        private static async Task DailyProcessAsync()
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
            await using (var dbContext = new ChapubelichdbContext())
            {
                if (dbContext.Configurations.First().LastResetTime > new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 00, 00, 00))
                    alreadyRestarted = true;
            }
            if (!alreadyRestarted)
                dailyResetTask = DailyResetJob.ExecuteManuallyAsync();

            //send compliments if not done before
            var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 00, 00);
            if (DateTime.Now > date)
                await DailyComplimentJob.ExecuteManuallyAsync(Client);

            if (dailyResetTask != null)
                await dailyResetTask;
        }
        private static async Task RestoreDataAsync()
        {
            List<RouletteGameSession> gameSessionsToResume;
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            {
                gameSessionsToResume =
                    await dbContext.RouletteGameSessions
                       .Include(gs => gs.ColorBetTokens)
                       .ThenInclude(bt => bt.User)
                       .Include(gs => gs.NumberBetTokens)
                       .ThenInclude(bt => bt.User)
                       .Where(gs => gs.Resulting)
                       .ToListAsync();
                await Task.Delay(5000);
            }

            Parallel.ForEach(gameSessionsToResume,
                async gs => await RouletteGameManager.ResumeResultingAsync(gs.ChatId));
        }

        private static async void MessageProcessAsync(object sender, MessageEventArgs e)
        {
            Console.WriteLine("{0:HH:mm:ss}: {1} {2}| {3} ({4} | {5}): [{6}] {7}", e.Message.Date, e.Message.Type,
                e.Message.From.Id, e.Message.From.Username,
                e.Message.Chat.Id, e.Message.Chat?.Title, e.Message.MessageId, e.Message.Text);

            if (e.Message.Date.AddMinutes(Config.GetValue<int>("BotSettings:MessageCheckPeriod")) < DateTime.UtcNow)
                return;

            if (await ChapubelichClient.AdminMessageProcessor.ExecuteAsync(e.Message, Client))
                return;
            foreach (var messageProcessor in ChapubelichClient.BotMessageProcessorsList)
                if (await messageProcessor.ExecuteAsync(e.Message, Client))
                    return;
        }
        private static async void CallbackProcess(object sender, CallbackQueryEventArgs e)
        {
            foreach (var messageProcessor in ChapubelichClient.BotCallbackMessageProcessorsList)
                if (await messageProcessor.ExecuteAsync(e.CallbackQuery, Client))
                    return;
        }
    }
}
