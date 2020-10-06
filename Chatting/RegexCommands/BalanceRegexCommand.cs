using Chapubelich.Abstractions;
using Chapubelich.ChapubelichBot.Init;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Chapubelich.Chatting.RegexCommands
{
    class BalanceRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/?(б|баланс|счет|balance)(@ChapubelichBot)?$";

        public override void Execute(Message message, ITelegramBotClient client)
        {
            var balanceCommand = Bot.BotPrivateCommandsList.First(x => x.Name == "\U0001F4B0 Баланс");
            if (null != balanceCommand)
                balanceCommand.Execute(message, client);
        }
    }
}
