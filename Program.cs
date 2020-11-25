using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using Microsoft.Extensions.Configuration;

namespace ChapubelichBot
{
    class Program
    {
        static async Task Main()
        {
            var config = ChapubelichClient.GetConfig();
            string path = Path.Combine(@"./Main/Data", config.GetValue<string>("Logger:DirectoryPath"), config.GetValue<string>("Logger:FileName"));
            Console.WriteLine($"Default log path is: {path}"); 
            AppDomain.CurrentDomain.UnhandledException += ExceptionManage;
            await ChapubelichInteractor.StartAsync();
            //var me = messageManager.client.GetMeAsync();
            //Console.Title = messageManager.client.GetMeAsync().Result.Username;
            Thread.Sleep(int.MaxValue);
        }
        private static void ExceptionManage(object sender, UnhandledExceptionEventArgs e)
        {   
            ChapubelichInteractor.Stop();
            var config = ChapubelichClient.GetConfig();
            string path = Path.Combine(@"./Main/Data", config.GetValue<string>("Logger:DirectoryPath"), config.GetValue<string>("Logger:FileName"));
            ChapubelichExceptionLogger exceptionLogger = new ChapubelichExceptionLogger((Exception)e.ExceptionObject);
            exceptionLogger.WriteData(path);
        }
    }
}
