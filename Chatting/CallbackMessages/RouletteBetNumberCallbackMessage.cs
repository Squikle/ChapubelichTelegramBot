using ChapubelichBot.Database;
using ChapubelichBot.Types.Abstractions;
using System.Threading.Tasks;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Games.RouletteGame;
using ChapubelichBot.Types.Statics;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = ChapubelichBot.Database.Models.User;

namespace ChapubelichBot.Chatting.CallbackMessages
{
    class RouletteBetNumbersCallbackMessage : CallBackMessage
    {
        public override List<string> IncludingData => new List<string> { 
            "rouletteBetEven", "rouletteBetOdd", 
            "rouletteBetFirstHalf", "rouletteBetSecondHalf",
             "rouletteBetFirstTwelve", "rouletteBetSecondTwelve", "rouletteBetThirdTwelve",
             "rouletteBetFirstRow", "rouletteBetSecondRow", "rouletteBetThirdRow"
        };
        public override async Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
            var gameSession = RouletteTableStatic.GetGameSessionByChatId(query.Message.Chat.Id);

            if (null == gameSession)
                return;
            if (gameSession.Resulting)
            {
                await client.TryAnswerCallbackQueryAsync(query.Id,
                        "Барабан уже крутится, слишком поздно для ставок");
                return;
            }

            using (var db = new ChapubelichdbContext())
            {
                User user = db.Users.FirstOrDefault(x => x.UserId == query.From.Id);
                if (user == null)
                    return;

                if (user.DefaultBet > user.Balance)
                {
                    await client.TryAnswerCallbackQueryAsync(query.Id,
                        "У вас недостаточно средств на счету");
                    return;
                }

                int[] userBets = RouletteTableStatic.GetBetsByCallbackQuery(query.Data);

                var numberBetTokens = gameSession.BetTokens.OfType<RouletteNumbersBetToken>();
                RouletteNumbersBetToken currentBetToken = numberBetTokens.FirstOrDefault(x => x.ChoosenNumbers.SequenceEqual(userBets) && x.UserId == user.UserId);

                if (currentBetToken != null)
                    currentBetToken.BetSum += user.DefaultBet;
                else
                {
                    currentBetToken = new RouletteNumbersBetToken(user, user.DefaultBet, userBets);
                    gameSession.BetTokens.Add(currentBetToken);
                }

                user.Balance -= user.DefaultBet;
                db.SaveChanges();

                string transactionResult = $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, ставка принята. Ваша суммарная ставка:"
                    + gameSession.UserBetsToStringAsync(user);

                await client.TrySendTextMessageAsync(
                    query.Message.Chat.Id,
                    transactionResult,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                await client.TryAnswerCallbackQueryAsync(query.Id);
            }
        }
    }
}
