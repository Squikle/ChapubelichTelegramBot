using System.Collections.Generic;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Entities.Roulette;
using ChapubelichBot.Types.Managers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.CallbackCommands.Roulette
{
    class BetColorCallback : CallbackCommand
    {
        public override List<string> IncludingData => new List<string> { "rouletteBetRed", "rouletteBetBlack", "rouletteBetGreen" };
        public override async Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
            RouletteGameSession gameSession = null;
            await using (var dbContext = new ChapubelichdbContext())
            {
                gameSession = await RouletteGameManager.GetGameSessionOrNullAsync(query.Message.Chat.Id, dbContext);
            }
            if (gameSession != null)
                await RouletteGameManager.BetColorRequestAsync(query);
        }
    }
}
