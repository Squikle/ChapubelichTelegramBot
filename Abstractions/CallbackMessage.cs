using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Chapubelich.Abstractions
{
    public abstract class CallBackMessage
    {
        public abstract List<string> IncludingData { get; }
        public abstract void Execute(CallbackQuery query, ITelegramBotClient client);
        public bool Contains(CallbackQuery query) =>
            IncludingData.Contains(query.Data);
    }
}
