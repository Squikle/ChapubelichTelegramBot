using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Entities;
using ChapubelichBot.Types.Managers.MessagesSender;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.Commands.Admin
{
    class CancelShutdown : Command
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
