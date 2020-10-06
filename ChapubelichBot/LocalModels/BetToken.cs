using Chapubelich.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapubelich.ChapubelichBot.LocalModels
{
    public class BetToken
    {
        public int UserId { get; set; }
        public int BetSum { get; set; }
        public int Choose { get; set; }

        public BetToken(User user, int bet, int choose)
        {
            UserId = user.UserId;
            BetSum = bet;
            Choose = choose;
        }
    }
}
