using System;
using System.IO;
using System.Text;
using System.Threading;
using ChapubelichBot.Init;
using ChapubelichBot.Types.ManageMessages;
using Microsoft.Extensions.Configuration;

namespace ChapubelichBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = Bot.GetConfig();
            string path = Path.Combine(@"./Init/Config", config.GetValue<string>("Logger:DirectoryPath"), config.GetValue<string>("Logger:FileName"));
            Console.WriteLine($"Default log path is: {path}"); 
            AppDomain.CurrentDomain.UnhandledException += ExceptionManage;
            BotProcessor.StartReceiving();
            throw new Exception("a", new Exception("b", new Exception("c", new Exception("d", new Exception("e", new Exception("a", new Exception("g")))))));
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
