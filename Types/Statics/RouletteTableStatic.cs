using ChapubelichBot.Types.Games.RouletteGame;
using System.Collections.Generic;
using System.Linq;
using ChapubelichBot.Types.Enums;
using ChapubelichBot.Database.Models;
using System;
using ChapubelichBot.Types.Abstractions;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot.Types;
using Telegram.Bot;
using ChapubelichBot.Types.Extensions;

namespace ChapubelichBot.Types.Statics
{
    static class RouletteTableStatic
    {
        public static string Name => "\U0001F525Рулетка\U0001F525";



        public const int tableSize = 37;
        public static List<RouletteGameSession> GameSessions { get; set; } = new List<RouletteGameSession>();
        public static int GetRandomResultNumber()
        {
            Random rand = new Random();
            return rand.Next(0, tableSize - 1);
        }
        public static RouletteGameSession GetGameSessionOrNull(long chatId)
        {
            return GameSessions.FirstOrDefault(x => x.ChatId == chatId);
        }
        public static string ToEmoji(this RouletteColorEnum color)
        {
            switch (color)
            {
                case RouletteColorEnum.Red:
                    return "\U0001F534";
                case RouletteColorEnum.Black:
                    return "\U000026AB";
                case RouletteColorEnum.Green:
                    return "\U0001F7E2";
                default:
                    return "";
            }
        }
        public static RouletteColorEnum ToRouletteColor(this int number)
        {
            RouletteColorEnum[] rouletteTable = new RouletteColorEnum[tableSize]
            {
                RouletteColorEnum.Green, //0
                RouletteColorEnum.Red,
                RouletteColorEnum.Black,
                RouletteColorEnum.Red,
                RouletteColorEnum.Black,
                RouletteColorEnum.Red, //5
                RouletteColorEnum.Black,
                RouletteColorEnum.Red,
                RouletteColorEnum.Black,
                RouletteColorEnum.Red,
                RouletteColorEnum.Black, //10
                RouletteColorEnum.Black,
                RouletteColorEnum.Red,
                RouletteColorEnum.Black,
                RouletteColorEnum.Red,
                RouletteColorEnum.Black, //15
                RouletteColorEnum.Red,
                RouletteColorEnum.Black,
                RouletteColorEnum.Red,
                RouletteColorEnum.Red,
                RouletteColorEnum.Black, //20
                RouletteColorEnum.Red,
                RouletteColorEnum.Black,
                RouletteColorEnum.Red,
                RouletteColorEnum.Black,
                RouletteColorEnum.Red, //25
                RouletteColorEnum.Black,
                RouletteColorEnum.Red,
                RouletteColorEnum.Black,
                RouletteColorEnum.Black,
                RouletteColorEnum.Red, //30
                RouletteColorEnum.Black,
                RouletteColorEnum.Red,
                RouletteColorEnum.Black,
                RouletteColorEnum.Red,
                RouletteColorEnum.Black, //35
                RouletteColorEnum.Red
            };
            return rouletteTable[number];
        }
        public static int[] GetBetsByNumbers(int firstNumber)
        {
            return new int[]
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
                    userBet = new int[(tableSize - 1) / 2];
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
                    userBet = new int[(tableSize - 1) / 2];
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
                    return GetBetsByNumbers(1, (tableSize - 1) / 2);
                case "rouletteBetSecondHalf":
                    return GetBetsByNumbers(((tableSize - 1) / 2) + 1, tableSize - 1);

                case "rouletteBetFirstTwelve":
                    return GetBetsByNumbers(1, (tableSize - 1) / 3);
                case "rouletteBetSecondTwelve":
                    int dividedByThree = (tableSize - 1) / 3; 
                    return GetBetsByNumbers(dividedByThree+1, dividedByThree * 2);
                case "rouletteBetThirdTwelve":
                    dividedByThree = (tableSize - 1) / 3;
                    return GetBetsByNumbers((dividedByThree * 2) + 1, dividedByThree * 3);

                case "rouletteBetFirstRow":
                    userBet = new int[(tableSize - 1) / 3];
                    for (int i = 0, j = 1; i < userBet.Length; i++)
                    {
                        userBet[i] = j;
                        j += 3;
                    }
                    return userBet;
                case "rouletteBetSecondRow":
                    userBet = new int[(tableSize - 1) / 3];
                    for (int i = 0, j = 2; i < userBet.Length; i++)
                    {
                        userBet[i] = j;
                        j += 3;
                    }
                    return userBet;
                case "rouletteBetThirdRow":
                    userBet = new int[(tableSize - 1) / 3];
                    for (int i = 0, j = 3; i < userBet.Length; i++)
                    {
                        userBet[i] = j;
                        j += 3;
                    }
                    return userBet;
            }

            return null;
        }
        public static IEnumerable<RouletteBetToken> GroupByUsers(this IEnumerable<RouletteBetToken> listOfTokens)
        {
            List<RouletteBetToken> groupedList = new List<RouletteBetToken>();

            foreach (var el in listOfTokens)
            {
                var token = groupedList.FirstOrDefault(x => x.UserId == el.UserId);
                if (token != null)
                    token.BetSum += el.BetSum;
                else groupedList.Add(el);
            }

            return groupedList;
        }
        public static bool IsSequenceBy(this int[] array, int increaseNumber)
        {
            if (array == null || array.Length <= 2) 
                return false;
            for (int i=0; i<array.Length-1; i++)
            {
                if (array[i + 1] - array[i] != increaseNumber)
                    return false;
            }
            return true;
        }

    }
}
