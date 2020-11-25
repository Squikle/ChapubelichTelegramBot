using System;
using System.Linq;
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
            foreach (var user in dbContext.Users)
            {
                user.Complimented = false;
                user.DailyRewarded = false;
            }
            (await dbContext.Configurations.FirstAsync()).LastResetTime = DateTime.Now;
            await dbContext.SaveChangesAsync();
            string dbSchema = ChapubelichClient.GetConfig().GetValue<string>("AppSettings:DatabaseSchema");
            await dbContext.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"{dbSchema}\".\"GroupDailyPerson\"");
        }
    }
}
