using System.Collections.Generic;
using System.Linq;
using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Enums;
using ChapubelichBot.Types.Statics;

namespace ChapubelichBot.Types.Extensions
{
    internal static class RouletteGameExtensions
    {
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
            for (int i = 0; i < array.Length - 1; i++)
            {
                if (array[i + 1] - array[i] != increaseNumber)
                    return false;
            }
            return true;
        }
        public static RouletteColorEnum ToRouletteColor(this int number)
        {
            RouletteColorEnum[] rouletteTable = new RouletteColorEnum[RouletteGame.TableSize]
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
    }
}
