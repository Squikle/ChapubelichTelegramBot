using System;
using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.RegexCommands.AdminRegexCommands
{
    class TestFeatureRegex : RegexCommand
    {
        public override string Pattern => @"/102938test";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            /*List<Task> sendTasks = new List<Task>(100);
            for (int i = 0; i < 100; i++)
            {
                int mes = i;
                Task task = client.TrySendTextMessageAsync(message.Chat.Id, mes.ToString());
                sendTasks.Add(task);
            }

            await Task.WhenAll(sendTasks.ToArray());*/

            Console.WriteLine("Test!");
        }
    }
}
