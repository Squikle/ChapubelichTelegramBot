using ChapubelichBot.Types.Enums;
using System;

namespace ChapubelichBot.Types.Statics
{
    static class RouletteColorStatic
    {
        private const int tableSize = 37;
        public static RouletteColorEnum GetColorByNumber(int number)
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
        public static string GetEmojiByColor(RouletteColorEnum color)
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
                    return null;
            }
        }
        public static int GetRandomResultNumber()
        {
            Random rand = new Random();
            return rand.Next(0, tableSize);
        }
    }
}
