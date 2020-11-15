using ChapubelichBot.Init;
using ChapubelichBot.Types.Abstractions;
using System.Threading.Tasks;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class BalanceRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *(б|баланс|счет|balance|b)(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            var balanceCommand = Bot.BotPrivateCommandsList.First(x => x.Name == "💰 Баланс");
            if (null != balanceCommand)
                await balanceCommand.ExecuteAsync(message, client);
        }
    }
}
