﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Entities;
using ChapubelichBot.Types.GameManagers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.CallbackCommands.Roulette
{
    class RouletteRollCallback : CallbackCommand
    {
        public override List<string> IncludingData => new List<string> {"rouletteRoll"};
        public override async Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
            RouletteGameSession gameSession = null;
            await using (var db = new ChapubelichdbContext())
            {
                gameSession = RouletteGameManager.GetGameSessionOrNull(query.Message.Chat.Id, db);
            }
            if (gameSession != null)
                await RouletteGameManager.RollRequest(query);
        }
    }
}