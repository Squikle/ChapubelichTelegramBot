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
            BotProcessor.StartReceiving();
            //var me = messageManager.client.GetMeAsync();
            //Console.Title = messageManager.client.GetMeAsync().Result.Username;
            Thread.Sleep(int.MaxValue);
            AppDomain.CurrentDomain.UnhandledException += ExceptionManage;
        }
        private static void ExceptionManage(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            string error = $"Time: {DateTime.Now}\n" +
                      $"Crashed: {ex.Source}\n" +
                      $"Error message: {ex.Message}\n" +
                      $"Last execute: {ex.StackTrace}\n";
            if (ex.InnerException != null)
                error += $"Inner exeption: {ex.InnerException}";
            error += "\n\n";

            var config = Bot.GetConfig();

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), config.GetValue<string>("Logger:DirectoryPath"));
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            using (var writer = new StreamWriter(Path.Combine(path, config.GetValue<string>("Logger:FileName")), true, Encoding.Default))
            {
                writer.WriteLine(error);
            }
        }
    }
}
