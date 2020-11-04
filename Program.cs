using System;
using System.IO;
using System.Threading;
using ChapubelichBot.Init;
using Microsoft.Extensions.Configuration;

namespace ChapubelichBot
{
    class Program
    {
        static void Main()
        {
            var config = Bot.GetConfig();
            string path = Path.Combine(@"./Init/Config", config.GetValue<string>("Logger:DirectoryPath"), config.GetValue<string>("Logger:FileName"));
            Console.WriteLine($"Default log path is: {path}"); 
            AppDomain.CurrentDomain.UnhandledException += ExceptionManage;
            BotProcessor.StartReceiving();
            //var me = messageManager.client.GetMeAsync();
            //Console.Title = messageManager.client.GetMeAsync().Result.Username;
            Thread.Sleep(int.MaxValue);
        }
        private static void ExceptionManage(object sender, UnhandledExceptionEventArgs e)
        {
            var config = Bot.GetConfig();
            string path = Path.Combine(@"./Init/Config", config.GetValue<string>("Logger:DirectoryPath"), config.GetValue<string>("Logger:FileName"));
            ExceptionLogger exceptionLogger = new ExceptionLogger((Exception)e.ExceptionObject);
            exceptionLogger.WriteData(path);
        }
    }
}
