using System.Collections.Generic;
using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Managers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.CallbackCommands.Crocodile
{
    class CrocodileHostCandidacyCallback : CallbackCommand
    {
        public override List<string> IncludingData => new List<string> { "hostCrocodileRequest" };
        public override async Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
            await CrocodileGameManager.AddToHostCandidatesRequestAsync(query);
        }
    }
}
