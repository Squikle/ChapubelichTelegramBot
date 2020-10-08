using ChapubelichBot.Init;
using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;

using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteStartRegexCommand : RegexCommand
    {
        public override string Pattern => @"^ *\/? *(рулетка|roulette)(@ChapubelichBot)?$";

        public override void Execute(Message message, ITelegramBotClient client)
        {
            var startCommand = Bot.BotPrivateCommandsList.First(x => x.Name == RouletteTableStatic.Name);
            if (null != startCommand)
                startCommand.Execute(message, client);
        }
    }
}
