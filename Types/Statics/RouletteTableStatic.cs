using ChapubelichBot.Types.Games.RouletteGame;
using System.Collections.Generic;
using System.Linq;
using ChapubelichBot.Types.Enums;
using ChapubelichBot.Database.Models;
using System;

namespace ChapubelichBot.Types.Statics
{
    static class RouletteTableStatic
    {
        public static string Name => "\U0001F525Рулетка\U0001F525";

        public const int tableSize = 37;
        public static int GetRandomResultNumber()
        {
            Random rand = new Random();
            return rand.Next(0, tableSize);
        }
        public static List<RouletteGameSession> GameSessions { get; set; } = new List<RouletteGameSession>();
        public static RouletteGameSession GetGameSessionByChatId(long chatId)
        {
            return GameSessions.FirstOrDefault(x => x.ChatId == chatId);
        }
        public static string ToEmoji(this RouletteColorEnum? color)
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
        public static RouletteColorEnum? ToRouletteColor(this int number)
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
        public static string UserBetsToString(this RouletteGameSession gameSession, User user)
        {
            string resultList = string.Empty;
            var userTokens = gameSession.BetTokens.Where(x => x.UserId == user.UserId);
            var colorUserTokens = userTokens.Where(x => x.ChoosenColor != null);

            var numberUserTokens = userTokens.Where(x => x.ChoosenNumbers != null);
            var oneNumberUserTokens = numberUserTokens.Where(x => x.ChoosenNumbers.Length == 1);
            var twoNumberUserTokens = numberUserTokens.Except(oneNumberUserTokens);

            foreach (var token in colorUserTokens)
            {
                switch (token.ChoosenColor)
                {
                    case RouletteColorEnum.Red:
                        resultList += $"\n<b>{token.BetSum} \U0001F534 </b>";
                        break;
                    case RouletteColorEnum.Black:
                        resultList += $"\n<b>{token.BetSum} \U000026AB </b>";
                        break;
                    case RouletteColorEnum.Green:
                        resultList += $"\n<b>{token.BetSum} \U0001F7E2 </b>";
                        break;
                }
            }

            foreach (var token in twoNumberUserTokens)
            {
                int firstnumber = token.ChoosenNumbers[0];
                int secondNumber = token.ChoosenNumbers[token.ChoosenNumbers.Length - 1];
                resultList += $"\n<b>{token.BetSum}</b> ({firstnumber} - {secondNumber})";
            }
            foreach (var token in oneNumberUserTokens)
            {
                int firstnumber = token.ChoosenNumbers[0];
                resultList += $"\n<b>{token.BetSum}</b> ({firstnumber})";
            }

            return resultList;
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
            return Enumerable.Range(firstNumber, secondNumber+1).ToArray();
        }
    }
}
