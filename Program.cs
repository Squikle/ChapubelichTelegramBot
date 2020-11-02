using System.Threading;
using ChapubelichBot.Types.ManageMessages;

namespace ChapubelichBot
{
    class Program
    {
        static void Main(string[] args)
        {
            BotManager.StartReceiving();
            //var me = messageManager.client.GetMeAsync();
            //Console.Title = messageManager.client.GetMeAsync().Result.Username;
            Thread.Sleep(int.MaxValue);
        }
    }
}
