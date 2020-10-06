using Chapubelich.ChapubelichBot.LocalModels;
using Chapubelich.Database;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = Chapubelich.Database.Models.User;

namespace Chapubelich.Abstractions
{
    public abstract class GameSession
    {
        public abstract Game CurrentGame { get; set;  }
        public abstract long ChatId { get; set; }
        public abstract List<BetToken> BetTokens { get; set; }
        public abstract Message GameMessage { get; set; }
        public abstract bool IsActive { get; set; }
        public GameSession(Message message)
        {
            BetTokens = new List<BetToken>();
            ChatId = message.Chat.Id;
        }

        public abstract void Start(Message message, ITelegramBotClient client);
        public abstract void Result(ITelegramBotClient client, CallbackQuery query = null, Message message = null);
    }
}
