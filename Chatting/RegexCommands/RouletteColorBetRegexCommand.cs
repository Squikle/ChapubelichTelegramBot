﻿using ChapubelichBot.Types.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using ChapubelichBot.Database;
using ChapubelichBot.Database.Models;
using ChapubelichBot.Types.Games.RouletteGame;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteColorBetRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/?(\d+) *(к(расный)?|ч(ерный)?|з(еленый)?|r(ed)?|b(lack)?|g(reen)?) *(го|ролл|погнали|крути|roll|go)?(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            RouletteGameSession gameSession = null;
            await using (var db = new ChapubelichdbContext())
            {
                gameSession = RouletteGameManager.GetGameSessionOrNull(message.Chat.Id, db);
            }
            if (gameSession != null)
                await RouletteGameManager.BetColorRequest(message, Pattern);
        }
    }
}
