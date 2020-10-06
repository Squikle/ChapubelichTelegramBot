using Chapubelich.Abstractions;
using Chapubelich.Extensions;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Chapubelich.Chatting.Commands.ShutdownCommands
{
    class SetShutdownCommand : Command
    {
        public override string Name => "/shutdown";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            if (message.Chat.Id == 243857110)
            {
                if (!ShutdownController.shuttingDown)
                {
                    string timerString = Regex.Match(message.Text, @"\d+").ToString();
                    int timerValue = int.TryParse(timerString, out timerValue) ? timerValue : ShutdownController.defaultShutdown;
                    System.Diagnostics.Process.Start("CMD.exe", "/C shutdown -s -t " + timerValue);
                    ShutdownController.shuttingDown = true;
                    await client.TrySendTextMessageAsync(message.Chat.Id, $"Таймер установлен на {timerValue / 60} минут.");
                }
            }
        }
    }
}
