using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.CallbackCommands
{
    class CrocodileHostingCallback : CallbackCommand
    {
        public override List<string> IncludingData => new List<string> { "hostCrocodile" };
        public override Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
            throw new NotImplementedException();
        }
    }
}
