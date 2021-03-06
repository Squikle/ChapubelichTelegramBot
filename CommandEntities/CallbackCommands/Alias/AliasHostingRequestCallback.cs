﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Managers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.CallbackCommands.Alias
{
    class AliasHostCandidacyCallback : CallbackCommand
    {
        public override List<string> IncludingData => new List<string> { "hostAliasRequest" };
        public override async Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
            await AliasGameManager.AddToHostCandidatesRequestAsync(query);
        }
    }
}
