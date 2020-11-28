using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Entities;
using ChapubelichBot.Types.Managers.MessagesSender;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Telegram.Bot;

namespace ChapubelichBot.Types.ScheduledJobs
{
    class DailyComplimentJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            ITelegramBotClient client = (ITelegramBotClient)context.JobDetail.JobDataMap["TelegramBotClient"];
            await ExecuteManuallyAsync(client);
        }
        public static async Task ExecuteManuallyAsync(ITelegramBotClient client)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} рассылаю комплименты...");

            string[] boyCompliments = null;
            string[] girlCompliments = null;
            User[] complimentingUsers;

            await using (var dbContext = new ChapubelichdbContext())
            {
                var complimentingUsersQuery = dbContext.Users
                    .Include(u => u.UserCompliment)
                    .Where(u => u.UserCompliment != null && !u.UserCompliment.Praised);

                complimentingUsers = complimentingUsersQuery.ToArray();
                if (complimentingUsers.Length <= 0)
                    return;

                if (complimentingUsers.Any(x => x.Gender))
                    boyCompliments = dbContext.BoyCompliments.Select(x => x.ComplimentText).ToArray();
                if (complimentingUsers.Any(x => x.Gender == false))
                    girlCompliments = dbContext.GirlCompliments.Select(x => x.ComplimentText).ToArray();
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
                await client.TrySendTextMessageAsync(uc.Key, $"🎉Твой комплимент дня🎉\n{uc.Value}"));

            await using (var dbContext = new ChapubelichdbContext())
            {
                foreach (var user in dbContext.Users
                    .Include(u => u.UserCompliment)
                    .Where(u => userCompliments.Keys.Contains(u.UserId)))
                {
                    user.UserCompliment.Praised = true;
                }

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
