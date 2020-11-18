using ChapubelichBot.Database.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Types.Games.RouletteGame
{
    public class RouletteGameSessionBuilder
    {
        private RouletteGameSession _rouletteGameSession;

        private RouletteGameSessionBuilder() {}
        public static RouletteGameSessionBuilder Create()
        {
            return new RouletteGameSessionBuilder();
        }
        public RouletteGameSessionBuilder InitializeNew(Message messageFromChat, ITelegramBotClient client)
        {
            _rouletteGameSession = new RouletteGameSession(messageFromChat.Chat.Id, client);
            return this;
        }
        public RouletteGameSessionBuilder RestoreFrom(RouletteGameSessionData restoringGameSessionData, ITelegramBotClient client)
        {
            _rouletteGameSession = new RouletteGameSession(restoringGameSessionData, client);
            return this;
        }
        public RouletteGameSessionBuilder AddToSessionsList()
        {
            Statics.RouletteGame.GameSessions.Add(_rouletteGameSession);
            return this;
        }
        public RouletteGameSession Build()
        {
            return _rouletteGameSession;
        }
    }
}
