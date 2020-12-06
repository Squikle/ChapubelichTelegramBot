using System;
using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Managers;
using ChapubelichBot.Types.Managers.MessagesSender;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.RegexCommands.Crocodile
{
    class CrocodileStartRegex : RegexCommand
    {
        public override string Pattern => @"^\/? *(крокодил|crocodile)(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await CrocodileGameManager.CreateRequestAsync(message);
        }
    }
}
