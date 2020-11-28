using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Entities;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Managers.MessagesSender;
using ChapubelichBot.Types.Statics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace ChapubelichBot.Types.Managers
{
    class CrocodileGameManager
    {
        public static string Name => "\U0001F40AКрокодил\U0001F40A";

        // Fields
        private static ITelegramBotClient Client => ChapubelichClient.GetClient();
        private static Timer _deadSessionsCollector;

        // C-tor
        public static void Init()
        {
            int periodToCollect = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
            _deadSessionsCollector = new Timer(async _ => await CollectDeadSessionsAsync(), null, periodToCollect, periodToCollect);
        }
        public static void Terminate()
        {
            _deadSessionsCollector.Dispose();
        }
        // TODO: переделать метод
        private static async Task CollectDeadSessionsAsync()
        {
            List<RouletteGameSession> deadSessions;
            await using (var dbContext = new ChapubelichdbContext())
            {
                int timeToSessionDispose = ChapubelichClient.GetConfig().GetValue<int>("AppSettings:StopGameDelay");

                deadSessions = (await dbContext.RouletteGameSessions
                    .Where(gs => gs.LastActivity < DateTime.UtcNow && !gs.Resulting)
                    .ToListAsync())
                    .Where(gs => gs.LastActivity.AddSeconds(timeToSessionDispose) < DateTime.UtcNow)
                    .ToList();
            }

            Parallel.ForEach(deadSessions, async gs =>
            {
                await using ChapubelichdbContext dbContext = new ChapubelichdbContext();
                gs = await GetGameSessionOrNullAsync(gs.ChatId, dbContext);

                var returnedBets = string.Empty;

                var groupedColorBetList = gs.ColorBetTokens.GroupByUsers().ToList();
                var groupedNumberBetList = gs.NumberBetTokens.GroupByUsers().ToList();
                if (groupedColorBetList.Any() || groupedNumberBetList.Any())
                {
                    foreach (var bet in groupedColorBetList)
                    {
                        var user = bet.User;
                        if (user != null)
                            user.Balance += bet.BetSum;
                    }

                    foreach (var bet in groupedNumberBetList)
                    {
                        var user = bet.User;
                        if (user != null)
                            user.Balance += bet.BetSum;
                    }
                    returnedBets += "\nСтавки были возвращены 👍";
                }
                Task deletingMessage = null;
                if (gs.GameMessageId != 0)
                    deletingMessage = Client.TryDeleteMessageAsync(gs.ChatId, gs.GameMessageId);
                Task sendingMessage = Client.TrySendTextMessageAsync(
                    gs.ChatId,
                    "Игровая сессия отменена из-за отсутствия активности" + returnedBets,
                    ParseMode.Html,
                    replyMarkup: InlineKeyboards.RoulettePlayAgainMarkup);

                dbContext.RouletteGameSessions.Remove(gs);
                try
                {
                    await dbContext.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    return;
                }

                if (deletingMessage != null)
                    await deletingMessage;

                await sendingMessage;
            });
        }
    }
}
