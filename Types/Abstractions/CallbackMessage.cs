﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Types.Abstractions
{
    public abstract class CallBackMessage
    {
        public abstract List<string> IncludingData { get; }
        public abstract Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client);
        public bool Contains(CallbackQuery query) =>
            IncludingData.Contains(query.Data);
    }
}
