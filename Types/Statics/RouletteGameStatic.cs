using ChapubelichBot.Types.Games.RouletteGame;
using ChapubelichBot.Database.Models;
using System.Collections.Generic;
using System.Linq;
using ChapubelichBot.Types.Enums;

namespace ChapubelichBot.Types.Statics
{
    static class RouletteGameStatic
    {
        public static string Name => "\U0001F525Рулетка\U0001F525";
        public static List<RouletteGameSession> GameSessions { get; set; } = new List<RouletteGameSession>();
        public static RouletteGameSession GetGameSessionByChatId(long chatId)
        {
            return GameSessions.FirstOrDefault(x => x.ChatId == chatId);
        }
        public static string GetUserListOfBets(User user, RouletteGameSession gameSession)
        {
            string resultList = string.Empty;
            var userTokens = gameSession.BetTokens.Where(x => x.UserId == user.UserId);
            foreach (var token in userTokens)
            {
                switch (token.ColorChoose)
                {
                    case RouletteColorEnum.Red:
                        resultList += $"<b>\n\U0001F534 {token.BetSum}</b>";
                        break;
                    case RouletteColorEnum.Black:
                        resultList += $"<b>\n\U000026AB {token.BetSum}</b>";
                        break;
                    case RouletteColorEnum.Green:
                        resultList += $"<b>\n\U0001F7E2 {token.BetSum}</b>";
                        break;
                }
            }

            return resultList;
        }
    }
}
