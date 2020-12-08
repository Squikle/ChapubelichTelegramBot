using System;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Quartz;

namespace ChapubelichBot.Types.ScheduledJobs
{
    class DailyResetJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await ExecuteManuallyAsync();
        }
        public static async Task ExecuteManuallyAsync()
        {
            Console.WriteLine($"{DateTime.Now} дневной сброс...");
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
            foreach (var user in dbContext.Users
                .Include(u => u.DailyReward)
                .ThenInclude(dr => dr.User)
                .ThenInclude(u => u.DailyReward)
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

            bool saved = false;
            while (!saved)
            {
                try
                {
                    await dbContext.SaveChangesAsync();
                    saved = true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries)
                    {
                        if (entry.Entity is UserDailyReward userDailyReward)
                        {
                            await entry.ReloadAsync();
                            Console.WriteLine("Конфликт параллелизма для сброса ежедневной награды (DailyResetJob)");
                            if (userDailyReward.User.DailyReward == null)
                                continue;

                            if (!userDailyReward.User.DailyReward.Rewarded ||
                                userDailyReward.User.DailyReward.Stage >= 6)
                            {
                                dbContext.Remove(userDailyReward.User.DailyReward);
                                Console.WriteLine("Сработало");
                            }
                            else
                            {
                                userDailyReward.Rewarded = false;
                                userDailyReward.Stage++;
                            }
                        }
                    }
                }
                catch (DbUpdateException ex)
                {
                    foreach (var entry in ex.Entries)
                    {
                        if (entry.Entity is UserDailyReward)
                            Console.WriteLine("Повторное удаление ежедневной награды");
                    }
                }
            }

            string dbSchema = ChapubelichClient.GetConfig().GetValue<string>("BotSettings:DatabaseSchema");
            await dbContext.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"{dbSchema}\".\"GroupDailyPerson\"");
        }
    }
}
