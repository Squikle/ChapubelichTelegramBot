using Chapubelich.Abstractions;
using Chapubelich.ChapubelichBot.Init;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Chapubelich.Chatting.Games.FiftyFiftyGame
{
    class FiftyFiftyStartRegexCommand : RegexCommand
    {
        public override string Pattern => @"^ *\/? *(рулетка|roulette)(@ChapubelichBot)?$";

        public override void Execute(Message message, ITelegramBotClient client)
        {
            var startCommand = Bot.BotPrivateCommandsList.First(x => x.Name == FiftyFiftyGame.Name);
            if (null != startCommand)
                startCommand.Execute(message, client);
        }
    }
}
