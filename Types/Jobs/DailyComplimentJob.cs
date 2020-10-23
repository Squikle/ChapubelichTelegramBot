using ChapubelichBot.Database;
using ChapubelichBot.Database.Models;
using ChapubelichBot.Types.Extensions;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace ChapubelichBot.Types.Jobs
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
            Console.WriteLine($"{DateTime.Now} рассылаю комплименты...");

            string[] boyCompliments = null;
            string[] girlCompliments = null;
            User[] complimentingUsers;

            using (var db = new ChapubelichdbContext())
            {
                var complimentingUsersDatabase = db.Users.Where(x => x.ComplimentSubscription == true && x.IsAvailable && !x.Complimented);

                complimentingUsers = complimentingUsersDatabase.ToArray();
                if (complimentingUsers.Length <= 0)
                    return;

                foreach (var user in complimentingUsersDatabase)
                {
                    user.Complimented = true;
                }
                await db.SaveChangesAsync();

                if (complimentingUsers.Any(x => x.Gender == true))
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
                        compliment = boyCompliments[rand.Next(0, boyCompliments.Length)];
                        break;
                    case false:
                        compliment = girlCompliments[rand.Next(0, girlCompliments.Length)];
                        break;
                    default:
                        compliment = "Твои глаза прекрасны😍";
                        break;
                }
                userCompliments.Add(user.UserId, compliment);
            }

            // M compliment to console -------------------
            int mUser = userCompliments.Keys.FirstOrDefault(x => x == 583067838);
            if (mUser != 0)
                Console.WriteLine(userCompliments[mUser]);
            // -------------------------------------------

            foreach (var userCompliment in userCompliments)
            {
                await client.TrySendTextMessageAsync(userCompliment.Key, $"❤️Твой комплимент дня❤️\n{userCompliment.Value}");
            }
        }
    }
}
