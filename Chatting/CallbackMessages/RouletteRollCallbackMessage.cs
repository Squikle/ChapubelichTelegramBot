using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.CallbackMessages
{
    class RouletteRollCallbackMessage : CallBackMessage
    {
        public override List<string> IncludingData => new List<string> {"rouletteRoll"};

        public override async void Execute(CallbackQuery query, ITelegramBotClient client)
        {
            var gameSession = RouletteTableStatic.GetGameSessionByChatId(query.Message.Chat.Id);
            if (gameSession == null)
                return;

            if (!gameSession.BetTokens.Any(x => x.UserId == query.From.Id))
            {
                await client.TryAnswerCallbackQueryAsync(
                query.Id,
                "Чтобы крутить барабан сделайте ставку");
                return;
            }

            gameSession.Result(client);
            await client.TryAnswerCallbackQueryAsync(query.Id);
        }
    }
}
