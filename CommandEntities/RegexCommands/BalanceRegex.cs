using System.Linq;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.RegexCommands
{
    class BalanceRegex : RegexCommand
    {
        public override string Pattern => @"^\/? *(б|баланс|счет|balance|b)(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            var balanceCommand = ChapubelichClient.BotPrivateCommandsList.First(x => x.Name == "💰 Баланс");
            if (null != balanceCommand)
                await balanceCommand.ExecuteAsync(message, client);
        }
    }
}
