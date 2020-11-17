using ChapubelichBot.Types.Games.RouletteGame;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ChapubelichBot.Types.Statics
{
    static class RouletteGame
    {
        public static string Name => "\U0001F525Рулетка\U0001F525";

        public const int TableSize = 37;
        public static List<RouletteGameSession> GameSessions { get; set; } = new List<RouletteGameSession>();
        public static int GetRandomResultNumber()
        {
            Random rand = new Random();
            return rand.Next(0, TableSize - 1);
        }
        public static RouletteGameSession GetGameSessionOrNull(long chatId)
        {
            return GameSessions.FirstOrDefault(x => x.ChatId == chatId);
        }
        public static int[] GetBetsByNumbers(int firstNumber)
        {
            return new[]
            {
                firstNumber
            };
        }
        public static int[] GetBetsByNumbers(int firstNumber, int secondNumber)
        {
            int betSize = secondNumber - firstNumber + 1;
            int[] bets = new int[betSize];
            for (int i = 0; i < betSize; i++)
            {
                bets[i] = firstNumber + i;
            }
            return bets;
        }
        public static int[] GetBetsByCallbackQuery(string queryData)
        {
            int[] userBet;
            switch (queryData)
            {
                case "rouletteBetEven":
                    userBet = new int[(TableSize - 1) / 2];
                    for (int i = 0, j = 1; i < userBet.Length; j++)
                    {
                        if (j % 2 == 0)
                        {
                            userBet[i] = j;
                            i++;
                        }
                    }
                    return userBet;
                case "rouletteBetOdd":
                    userBet = new int[(TableSize - 1) / 2];
                    for (int i = 0, j = 1; i < userBet.Length; j++)
                    {
                        if (j % 2 != 0)
                        {
                            userBet[i] = j;
                            i++;
                        }
                    }
                    return userBet;

                case "rouletteBetFirstHalf":
                    return GetBetsByNumbers(1, (TableSize - 1) / 2);
                case "rouletteBetSecondHalf":
                    return GetBetsByNumbers(((TableSize - 1) / 2) + 1, TableSize - 1);

                case "rouletteBetFirstTwelve":
                    return GetBetsByNumbers(1, (TableSize - 1) / 3);
                case "rouletteBetSecondTwelve":
                    int dividedByThree = (TableSize - 1) / 3; 
                    return GetBetsByNumbers(dividedByThree+1, dividedByThree * 2);
                case "rouletteBetThirdTwelve":
                    dividedByThree = (TableSize - 1) / 3;
                    return GetBetsByNumbers((dividedByThree * 2) + 1, dividedByThree * 3);

                case "rouletteBetFirstRow":
                    userBet = new int[(TableSize - 1) / 3];
                    for (int i = 0, j = 1; i < userBet.Length; i++)
                    {
                        userBet[i] = j;
                        j += 3;
                    }
                    return userBet;
                case "rouletteBetSecondRow":
                    userBet = new int[(TableSize - 1) / 3];
                    for (int i = 0, j = 2; i < userBet.Length; i++)
                    {
                        userBet[i] = j;
                        j += 3;
                    }
                    return userBet;
                case "rouletteBetThirdRow":
                    userBet = new int[(TableSize - 1) / 3];
                    for (int i = 0, j = 3; i < userBet.Length; i++)
                    {
                        userBet[i] = j;
                        j += 3;
                    }
                    return userBet;
            }

            return null;
        }
    }
}
