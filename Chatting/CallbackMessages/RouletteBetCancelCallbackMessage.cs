using ChapubelichBot.Database;
using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = ChapubelichBot.Database.Models.User;

namespace ChapubelichBot.Chatting.CallbackMessages
{
    class RouletteBetCancelCallbackMessage : CallBackMessage
    {
        public override List<string> IncludingData => new List<string> { "rouletteBetsCancel" };
        public override async Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
            var gameSession = RouletteTableStatic.GetGameSessionByChatId(query.Message.Chat.Id);
            if (gameSession == null || gameSession.Resulting)
                return;

            using (var db = new ChapubelichdbContext())
            {
                User user = db.Users.First(x => x.UserId == query.From.Id);
                if (user == null)
                    return;
                var userTokens = gameSession.BetTokens.Where(x => x.UserId == query.From.Id);

                if (userTokens.Any())
                {
                    foreach (var token in userTokens)
                    {
                        user.Balance += token.BetSum;
                    }
                    gameSession.BetTokens = gameSession.BetTokens.Except(userTokens).ToList();

                    db.SaveChanges();

                    await client.TrySendTextMessageAsync(
                            gameSession.ChatId,
                            $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, Ваша ставка отменена \U0001F44D",
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

                    await client.TryAnswerCallbackQueryAsync(query.Id, "✅");
                }
                else 
                    await client.TryAnswerCallbackQueryAsync(query.Id, "у Вас нет активных ставок");
            }
        }
    }
}
