﻿using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Managers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.RegexCommands.Roulette
{
    class RouletteStartRegex : RegexCommand
    {
        public override string Pattern => @"^\/? *(рулетка|казино|roulette|casino)(@ChapubelichBot)?$";

        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await RouletteGameManager.CreateRequestAsync(message);
        }
    }
}
