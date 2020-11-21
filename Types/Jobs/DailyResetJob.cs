using ChapubelichBot.Database;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;
using ChapubelichBot.Init;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ChapubelichBot.Types.Jobs
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
            await using ChapubelichdbContext db = new ChapubelichdbContext();
            foreach (var user in db.Users)
            {
                user.Complimented = false;
                user.DailyRewarded = false;
            }
            db.Configurations.First().LastResetTime = DateTime.Now;
            await db.SaveChangesAsync();
            string dbSchema = Bot.GetConfig().GetValue<string>("AppSettings:DatabaseSchema");
            db.Database.ExecuteSqlRaw($"TRUNCATE TABLE \"{dbSchema}\".\"GroupDailyPerson\"");
        }
    }
}
