using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;

namespace ChapubelichBot.Chatting.Commands.AdminCommands
{
    class CancelShutdownCommand : Command
    {
        public override string Name => "/cancelShutdown";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            if (message.Chat.Id != 243857110)
                return;

            if (ShutdownController.ShuttingDown)
            {
                System.Diagnostics.Process.Start("CMD.exe", "/C shutdown -a");
                await client.TrySendTextMessageAsync(message.Chat.Id, "Таймер отменен.");
                ShutdownController.ShuttingDown = false;
            }
            else await client.TrySendTextMessageAsync(message.Chat.Id, "Таймер не был инициализирован.");
        }
    }
}
