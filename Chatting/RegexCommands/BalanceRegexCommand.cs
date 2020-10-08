using ChapubelichBot.Init;
using ChapubelichBot.Types.Abstractions;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class BalanceRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *(б|баланс|счет|balance)(@ChapubelichBot)?$";

        public override void Execute(Message message, ITelegramBotClient client)
        {
            var balanceCommand = Bot.BotPrivateCommandsList.First(x => x.Name == "\U0001F4B0 Баланс");
            if (null != balanceCommand)
                balanceCommand.Execute(message, client);
        }
    }
}
