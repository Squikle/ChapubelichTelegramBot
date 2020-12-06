using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Managers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.Commands
{
    class RouletteStart : Command
    {
        public override string Name => RouletteGameManager.Name;
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await RouletteGameManager.CreateRequestAsync(message);
        }
    }
}
