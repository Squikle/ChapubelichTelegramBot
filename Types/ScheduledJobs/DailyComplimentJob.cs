﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Entities;
using ChapubelichBot.Types.Extensions;
using Quartz;
using Telegram.Bot;

namespace ChapubelichBot.Types.ScheduledJobs
{
    class DailyComplimentJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            ITelegramBotClient client = (ITelegramBotClient)context.JobDetail.JobDataMap["TelegramBotClient"];
            await ExecuteManually(client);
        }
        public static async Task ExecuteManually(ITelegramBotClient client)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} рассылаю комплименты...");

            string[] boyCompliments = null;
            string[] girlCompliments = null;
            User[] complimentingUsers;

            await using (var db = new ChapubelichdbContext())
            {
                var complimentingUsersDatabase = db.Users.Where(x => x.ComplimentSubscription && !x.Complimented);

                complimentingUsers = complimentingUsersDatabase.ToArray();
                if (complimentingUsers.Length <= 0)
                    return;

                foreach (var user in complimentingUsersDatabase)
                {
                    user.Complimented = true;
                }
                await db.SaveChangesAsync();

                if (complimentingUsers.Any(x => x.Gender))
                    boyCompliments = db.BoyCompliments.Select(x => x.ComplimentText).ToArray();
                if (complimentingUsers.Any(x => x.Gender == false))
                    girlCompliments = db.GirlCompliments.Select(x => x.ComplimentText).ToArray();
            }

            Random rand = new Random();
            Dictionary<int, string> userCompliments = new Dictionary<int, string>();
            foreach (var user in complimentingUsers)
            {
                string compliment;
                switch (user.Gender)
                {
                    case true:
                        compliment = boyCompliments?[rand.Next(0, boyCompliments.Length)];
                        break;
                    case false:
                        compliment = girlCompliments?[rand.Next(0, girlCompliments.Length)];
                        break;
                }
                userCompliments.Add(user.UserId, compliment);
            }

            // M compliment to console -------------------
            int mUser = userCompliments.Keys.FirstOrDefault(x => x == 583067838);
            if (mUser != 0)
                Console.WriteLine(userCompliments[mUser]);
            // -------------------------------------------

            Parallel.ForEach(userCompliments, async uc =>
                await client.TrySendTextMessageAsync(uc.Key, $"❤️Твой комплимент дня❤️\n{uc.Value}"));
        }
    }
}