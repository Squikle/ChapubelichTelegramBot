using ChapubelichBot.Database;
using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Enums;
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
    class RouletteBetColorCallbackMessage : CallBackMessage
    {
        public override List<string> IncludingData => new List<string> { "rouletteBetRed", "rouletteBetBlack", "rouletteBetGreen" };

        public override async void Execute(CallbackQuery query, ITelegramBotClient client)
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

                RouletteColorEnum playerChoose = RouletteColorEnum.Red;


                switch (query.Data)
                {
                    case "rouletteBetRed":
                        playerChoose = RouletteColorEnum.Red;
                        break;
                    case "rouletteBetBlack":
                        playerChoose = RouletteColorEnum.Black;
                        break;
                    case "rouletteBetGreen":
                        playerChoose = RouletteColorEnum.Green;
                        break;
                    default: return;
                }

                var colorBetTokens = gameSession.BetTokens.OfType<RouletteColorBetToken>();
                RouletteColorBetToken currentBetToken = colorBetTokens.FirstOrDefault(x => x.ChoosenColor == playerChoose && x.UserId == user.UserId);

                if (currentBetToken != null)
                    currentBetToken.BetSum += user.DefaultBet;
                else
                {
                    currentBetToken = new RouletteColorBetToken(user, user.DefaultBet, playerChoose);
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
