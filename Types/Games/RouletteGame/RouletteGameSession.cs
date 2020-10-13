using ChapubelichBot.Types.Statics;
using ChapubelichBot.Database;
using ChapubelichBot.Types.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using User = ChapubelichBot.Database.Models.User;
using ChapubelichBot.Types.Enums;
using ChapubelichBot.Types.Abstractions;
using System.Threading.Tasks;
using System.Text;

namespace ChapubelichBot.Types.Games.RouletteGame
{
    class RouletteGameSession
    {
        public long ChatId { get; set; }
        public List<RouletteBetToken> BetTokens { get; set; }
        public bool Resulting { get; set; }
        public int ResultNumber { get; set; }
        public Message GameMessage { get; set; }

        public async static Task<RouletteGameSession> Initialize(ITelegramBotClient client, Message message)
        {
            RouletteGameSession gameSession = new RouletteGameSession();
            gameSession.BetTokens = new List<RouletteBetToken>();
            gameSession.ChatId = message.Chat.Id;
            await gameSession.StartAsync(client, message);
            return gameSession;
        }

        private async Task StartAsync(ITelegramBotClient client, Message message)
        {
            Resulting = false;

            ResultNumber = RouletteTableStatic.GetRandomResultNumber();
            int replyId = message.From.Id == client.BotId ? 0 : message.MessageId;

            GameMessage = await client.TrySendPhotoAsync(message.Chat.Id,
                "https://i.imgur.com/SN8DRoa.png",
                caption: "Игра запущена. Ждем ваши ставки...\n" +
                "Вы можете поставить ставку по умолчанию на предложенные ниже варианты:",
                replyToMessageId: replyId,
                replyMarkup: InlineKeyboardsStatic.rouletteBetsMarkup);
        }
        public async Task ResultAsync(ITelegramBotClient client, Message startMessage = null)
        {
            if (Resulting)
                return;

            Resulting = true;
            Message animationMessage = await client.TrySendAnimationAsync(ChatId, GetRandomAnimationLink(), disableNotification: true, caption: "Крутим барабан...");

            Task task = Task.Delay(3000);
            // Удаление сообщений и отправка результатов
            string result = SummarizeAsync().ToString();
            await task;
            
            if (animationMessage != null)
                await client.TryDeleteMessageAsync(animationMessage.Chat.Id, animationMessage.MessageId);

            if (GameMessage != null)
                await client.TryDeleteMessageAsync(GameMessage.Chat.Id, GameMessage.MessageId);

            int replyId = startMessage == null ? 0 : startMessage.MessageId;
            await client.TrySendTextMessageAsync(
                ChatId,
                result,
                Telegram.Bot.Types.Enums.ParseMode.Html,
                replyMarkup: InlineKeyboardsStatic.roulettePlayAgainMarkup,
                replyToMessageId: replyId);

            RouletteTableStatic.GameSessions.Remove(this);
        }
        private StringBuilder SummarizeAsync()
        {
            RouletteColorEnum resultColor = ResultNumber.ToRouletteColor();

            StringBuilder result = new StringBuilder("Игра окончена.\nРезультат: ");
            result.Append($"{ResultNumber} {resultColor.ToEmoji()}");

            // Токены с цветом
            var winTokensColor = BetTokens.OfType<RouletteColorBetToken>().Where(x => x.ChoosenColor == resultColor).ToList();
            var looseTokensColor = BetTokens.OfType<RouletteColorBetToken>().Where(x => x.ChoosenColor != resultColor).ToList();

            // Токены с числами
            var winTokensNumbers = BetTokens.OfType<RouletteNumbersBetToken>().Where(x => x.ChoosenNumbers != null && x.ChoosenNumbers.Contains(ResultNumber)).ToList();
            var looseTokensNumbers = BetTokens.OfType<RouletteNumbersBetToken>().Where(x => x.ChoosenNumbers != null && !x.ChoosenNumbers.Contains(ResultNumber)).ToList();

            var winTokens = new List<RouletteBetToken>(winTokensColor.Count + winTokensColor.Count);
            winTokens.AddRange(winTokensColor);
            winTokens.AddRange(winTokensNumbers);

            var looseTokens = new List<RouletteBetToken>(looseTokensColor.Count + looseTokensNumbers.Count);
            looseTokens.AddRange(looseTokensColor);
            looseTokens.AddRange(looseTokensNumbers);

            using (var db = new ChapubelichdbContext())
            {
                // Определение победителей
                if (winTokens.Any())
                {
                    result.Append("\n🏆<b>Выиграли:</b>");
                    foreach (var token in winTokens.GroupByUsers())
                    {
                        int gainSum = token.GetGainSum();
                        User user = db.Users.FirstOrDefault(x => x.UserId == token.UserId);
                        result.Append($"\n<b>·</b><a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>: <b>+{gainSum.ToMoneyFormat()}</b>💵");
                        user.Balance += gainSum + token.BetSum;
                    }
                }
                // Определение проигравших
                if (looseTokens.Any())
                {
                    result.Append("\n\U0001F614<b>Проиграли:</b>");
                    foreach (var player in looseTokens.GroupByUsers())
                    {
                        User user = db.Users.FirstOrDefault(x => x.UserId == player.UserId);
                        result.Append($"\n<b>·</b><a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>: <b>-{player.BetSum.ToMoneyFormat()}</b>💵");
                    }
                }

                db.SaveChanges();
            }

            return result;
        }
        private static InputOnlineFile GetRandomAnimationLink()
        {
            string[] animationsLinks =
            {
                "https://media.giphy.com/media/uYDwaaoGJY26QDGzWr/giphy-downsized.gif",
                "https://media.giphy.com/media/erhiQf3RxVD6Sc8wXL/giphy-downsized.gif",
                "https://media.giphy.com/media/E8ucUBt3iTSR2Os1Dz/giphy.gif",
                "https://media.giphy.com/media/zDPm9BrrKWZqxGiyxG/giphy.gif",
                "https://media.giphy.com/media/XQyukWswMTViqZKft8/giphy.gif",
                "https://media.giphy.com/media/ats5YZpBxdvEpxLvcd/giphy.gif",
                "https://media.giphy.com/media/Bcwk17rD2eROmcZ1Di/giphy.gif",
                "https://media.giphy.com/media/MsUVF6GCUwEAZhNeTp/giphy.gif"
            };

            Random random = new Random();
            return new InputOnlineFile(animationsLinks[random.Next(0, animationsLinks.Length)]);
        }
        public StringBuilder UserBetsToStringAsync(User user)
        {
            StringBuilder resultList = new StringBuilder();
            var userTokens = BetTokens.Where(x => x.UserId == user.UserId);

            var colorUserTokens = userTokens.OfType<RouletteColorBetToken>();

            var numberUserTokens = userTokens.OfType<RouletteNumbersBetToken>();
            var oneNumberUserTokens = numberUserTokens.Where(x => x.ChoosenNumbers?.Length == 1);
            var rangeUserTokens = numberUserTokens.Except(oneNumberUserTokens);

            foreach (var token in colorUserTokens)
            {
                switch (token.ChoosenColor)
                {
                    case RouletteColorEnum.Red:
                        resultList.Append($"\n<b>{token.BetSum.ToMoneyFormat()}</b>: \U0001F534");
                        break;
                    case RouletteColorEnum.Black:
                        resultList.Append($"\n<b>{token.BetSum.ToMoneyFormat()}</b>: \U000026AB");
                        break;
                    case RouletteColorEnum.Green:
                        resultList.Append($"\n<b>{token.BetSum.ToMoneyFormat()}</b>: \U0001F7E2");
                        break;
                }
            }

            foreach (var token in rangeUserTokens)
            {
                if (token.ChoosenNumbers.IsSequenceBy(1))
                {
                    int firstnumber = token.ChoosenNumbers[0];
                    int secondNumber = token.ChoosenNumbers[token.ChoosenNumbers.Length - 1];
                    resultList.Append($"\n<b>{token.BetSum.ToMoneyFormat()}</b>: ({firstnumber} - {secondNumber})");
                }
                else
                {
                    if (token.ChoosenNumbers == null || token.ChoosenNumbers.Length <= 1)
                        return resultList;

                    resultList.Append($"\n<b>{token.BetSum.ToMoneyFormat()}</b>: ({token.ChoosenNumbers[0]}");

                    for (int i = 1; i < token.ChoosenNumbers.Length; i++)
                    {
                        resultList.Append($", {token.ChoosenNumbers[i]}");
                    }
                    resultList.Append(")");
                }
            }
            foreach (var token in oneNumberUserTokens)
            {
                resultList.Append($"\n<b>{token.BetSum}</b> ({token.ChoosenNumbers[0]} {token.ChoosenNumbers[0].ToRouletteColor().ToEmoji()})");
            }

            return resultList;
        }
    }
}
