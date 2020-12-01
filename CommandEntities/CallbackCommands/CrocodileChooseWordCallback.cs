using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Managers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.CallbackCommands
{
    class CrocodileChooseWordCallback : CallbackCommand
    {
        public override List<string> IncludingData => new List<string> { "crocodileChooseFirstWord", "crocodileChooseSecondWord", "crocodileChooseThirdWord" };
        public override async Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
           await CrocodileGameManager.ChooseWordRequestTask(query);
        }
    }
}
