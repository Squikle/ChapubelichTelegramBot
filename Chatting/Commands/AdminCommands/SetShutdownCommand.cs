using ChapubelichBot.Types.Abstractions;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using ChapubelichBot.Types.Extensions;

namespace ChapubelichBot.Chatting.Commands.AdminCommands
{
    class SetShutdownCommand : Command
    {
        public override string Name => "/shutdown";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            if (message.Chat.Id != 243857110)
                return;

            if (!ShutdownController.ShuttingDown)
            {
                string timerString = Regex.Match(message.Text, @"\d+").ToString();
                int timerValue = int.TryParse(timerString, out timerValue) ? timerValue : ShutdownController.DefaultShutdown;
                System.Diagnostics.Process.Start("CMD.exe", "/C shutdown -s -t " + timerValue);
                ShutdownController.ShuttingDown = true;
                await client.TrySendTextMessageAsync(message.Chat.Id, $"Таймер установлен на {timerValue / 60} минут.");
            }
        }
    }
}
