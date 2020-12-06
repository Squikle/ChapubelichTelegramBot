using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Entities.Users;
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

            string[] boyCompliments;
            string[] girlCompliments;
            User[] complimentingUsers;

            await using (var dbContext = new ChapubelichdbContext())
            {
                var complimentingUsersQuery = dbContext.Users
                    .Include(u => u.UserCompliment)
                    .Where(u => u.UserCompliment != null && !u.UserCompliment.Praised);

                complimentingUsers = complimentingUsersQuery.ToArray();
                if (complimentingUsers.Length <= 0)
                    return;

                boyCompliments = await GetComplimentsAsync(@"./Resources/compliments/MaleCompliments.txt", complimentingUsers.Count(cu => cu.Gender));
                girlCompliments = await GetComplimentsAsync(@"./Resources/compliments/FemaleCompliments.txt", complimentingUsers.Count(cu => !cu.Gender));
            }

            Dictionary<int, string> userCompliments = new Dictionary<int, string>();

            int maleNum = 0;
            int femaleNum = 0;
            foreach (var user in complimentingUsers)
            {
                string compliment;
                switch (user.Gender)
                {
                    case true:
                    {
                        compliment = boyCompliments[maleNum];
                        maleNum++;
                        break;
                    }
                    case false:
                    {
                        compliment = girlCompliments[femaleNum];
                        femaleNum++;
                        break;
                    }
                }
                userCompliments.Add(user.UserId, compliment);
            }

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

        private static async Task<string[]> GetComplimentsAsync(string pathOfWordsFile, int count)
        {
            Random rand = new Random();
            string[] words = await System.IO.File.ReadAllLinesAsync(pathOfWordsFile);
            string[] selectedCompliments = new string[count];
            for (int i = 0; i < selectedCompliments.Length; i++)
                selectedCompliments[i] = words[rand.Next(words.Length)];
            return selectedCompliments;
        }
    }
}
