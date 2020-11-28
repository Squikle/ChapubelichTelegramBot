using System;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Quartz;

namespace ChapubelichBot.Types.ScheduledJobs
{
    class DailyResetJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await ExecuteManually();
        }
        public static async Task ExecuteManually()
        {
            Console.WriteLine($"{DateTime.Now} дневной сброс...");
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            foreach (var user in dbContext.Users
                .Include(u => u.DailyReward)
                .Include(u => u.UserCompliment))
            {
                if (user.UserCompliment != null)   
                    user.UserCompliment.Praised = false;
                if (user.DailyReward != null)
                {
                    if (!user.DailyReward.Rewarded || user.DailyReward.Stage >= 6)
                        dbContext.Remove(user.DailyReward);
                    else
                    {
                        user.DailyReward.Rewarded = false;
                        user.DailyReward.Stage++;
                    }
                }
            }
            (await dbContext.Configurations.FirstAsync()).LastResetTime = DateTime.Now;
            await dbContext.SaveChangesAsync();
            string dbSchema = ChapubelichClient.GetConfig().GetValue<string>("AppSettings:DatabaseSchema");
            await dbContext.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"{dbSchema}\".\"GroupDailyPerson\"");
        }
    }
}
