using Chapubelich.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapubelich.Chatting.Games.FiftyFiftyGame
{
    static class FiftyFiftyGame
    {
        public static string Name => "\U0001F534 50/50 \U000026AB";
        public static List<FiftyFiftyGameSession> GameSessions { get; set; } = new List<FiftyFiftyGameSession>();
        public static FiftyFiftyGameSession GetGameSessionByChatId(long chatId)
        {
            var gameSession = GameSessions.FirstOrDefault(x => x.ChatId == chatId);

            return gameSession;
        }
    }
}
