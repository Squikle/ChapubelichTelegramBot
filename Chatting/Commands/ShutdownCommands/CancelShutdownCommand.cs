using Chapubelich.Abstractions;
using Chapubelich.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Chapubelich.Chatting.Commands.ShutdownCommands
{
    class CancelShutdownCommand : Command
    {
        public override string Name => "/cancel";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            if (message.Chat.Id == 243857110)
            {
                if (ShutdownController.shuttingDown)
                {
                    System.Diagnostics.Process.Start("CMD.exe", "/C shutdown -a");
                    await client.TrySendTextMessageAsync(message.Chat.Id, $"Таймер отменен.");
                    ShutdownController.shuttingDown = false;
                }
                else await client.TrySendTextMessageAsync(message.Chat.Id, $"Таймер не был инициализирован.");
            }
        }
    }
}
