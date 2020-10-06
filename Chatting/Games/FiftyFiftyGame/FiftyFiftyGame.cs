using Chapubelich.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapubelich.Chatting.Games.FiftyFiftyGame
{
    class FiftyFiftyGame : Game
    {
        public override string Name => "\U0001F3B0 50/50";
        public override List<GameSession> GameSessions { get; set; }
    }
}
