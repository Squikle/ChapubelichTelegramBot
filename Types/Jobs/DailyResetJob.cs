using ChapubelichBot.Database;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChapubelichBot.Types.Jobs
{
    class DailyResetJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"{DateTime.Now} дневной сброс...");
            using (ChapubelichdbContext db = new ChapubelichdbContext())
            {
                foreach (var user in db.Users)
                {
                    user.Complimented = false;
                    user.DailyRewarded = false;
                }
                db.Configurations.First().LastResetTime = DateTime.Now;
                await db.SaveChangesAsync();
            }
        }
    }
}
