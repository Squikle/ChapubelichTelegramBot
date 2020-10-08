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

namespace ChapubelichBot.Types.Games.RouletteGame
{
    class RouletteGameSession
    {
        public long ChatId { get; set; }
        public List<RouletteBetToken> BetTokens { get; set; }
        public bool Resulting { get; set; }
        public int ResultNumber { get; set; }
        public Message GameMessage { get; set; }

        public RouletteGameSession(ITelegramBotClient client, Message message)
        {
            BetTokens = new List<RouletteBetToken>();
            ChatId = message.Chat.Id;
            Start(client, message);
        }

        private async void Start(ITelegramBotClient client, Message message)
        {
            Resulting = false;

            ResultNumber = RouletteTableStatic.GetRandomResultNumber();
            int replyId = message.From.Id == client.BotId ? 0 : message.MessageId;
            GameMessage = await client.TrySendTextMessageAsync(
                message.Chat.Id,
                "Игра запущена. Ждем ваши ставки: ",
                replyToMessageId: replyId);
        }
        public async void Result(ITelegramBotClient client, Message startMessage = null)
        {
            if (Resulting)
                return;

            Resulting = true;
            Message animationMessage = await client.TrySendAnimationAsync(ChatId, GetRandomAnimationLink(), disableNotification: true, caption: "Крутим барабан...");

            // Удаление сообщений и отправка результатов
            string result = GetResultMessage();

            Thread.Sleep(3000);
            await client.TryDeleteMessageAsync(animationMessage.Chat.Id, animationMessage.MessageId);
            await client.TryDeleteMessageAsync(GameMessage.Chat.Id, GameMessage.MessageId);

            await client.TrySendTextMessageAsync(
                ChatId,
                result,
                Telegram.Bot.Types.Enums.ParseMode.Html,
                replyMarkup: InlineKeyboardsStatic.roulettePlayAgainMarkup,
                replyToMessageId: startMessage.MessageId);

            RouletteTableStatic.GameSessions.Remove(this);
        }

        private string GetResultMessage()
        {
            RouletteColorEnum? resultColor = ResultNumber.ToRouletteColor();

            string result = "Игра окончена.\nРезультат: ";
            result += $"{ResultNumber} {resultColor.ToEmoji()}";

            // Токены с цветом
            var winTokensColor = BetTokens.Where(x => x.ChoosenColor == resultColor)?.ToArray();
            var looseTokensColor = new List<RouletteBetToken>();
            foreach (var el in BetTokens.Where(x => x.ChoosenColor != resultColor && x.ChoosenColor != null))
            {
                var token = looseTokensColor.FirstOrDefault(x => x.UserId == el.UserId);
                if (token != null)
                    token.BetSum += el.BetSum;
                else looseTokensColor.Add(el);
            }

            // Токены с числами
            /*var winTokensNumbers = BetTokens.Where(x => x.ChoosenNumbers.Contains(ResultNumber))?.ToArray();
            var looseTokensNumbers = new List<RouletteBetToken>();
            foreach (var el in BetTokens.Where(x => !x.ChoosenNumbers.Contains(ResultNumber)))
            {
                var token = looseTokensNumbers.FirstOrDefault(x => x.UserId == el.UserId);
                if (token != null)
                    token.BetSum += el.BetSum;
                else looseTokensNumbers.Add(el);
            }*/

            using (var db = new ChapubelichdbContext())
            {
                // Определение победителей
                if (winTokensColor.Any()) // || winTokensNumbers.Any())
                {
                    result += "\n🏆<b>Выиграли:</b>";
                    foreach (var token in winTokensColor)
                    {
                        int gainSum = GetGainSum(token.ChoosenColor, token.BetSum);
                        User user = db.Users.FirstOrDefault(x => x.UserId == token.UserId);
                        result += $"\n<b>·</b><a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>: <b>+{gainSum - token.BetSum}</b>💵";
                        user.Balance += gainSum;
                    }
                    /*foreach (var token in winTokensNumbers)
                    {
                        int gainSum = GetGainSum(token.ChoosenNumbers, token.BetSum);
                        User user = db.Users.FirstOrDefault(x => x.UserId == token.UserId);
                        result += $"\n<b>·</b><a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>: <b>+{gainSum - token.BetSum}</b>💵";
                        user.Balance += gainSum;
                    }*/
                }
                // Определение проигравших
                if (looseTokensColor.Any()) // || looseTokensNumbers.Any())
                {
                    result += "\n\U0001F614<b>Проиграли:</b>";
                    foreach (var player in looseTokensColor)
                    {
                        User user = db.Users.FirstOrDefault(x => x.UserId == player.UserId);
                        result += $"\n<b>·</b><a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>: <b>-{player.BetSum}</b>💵";
                    }
                    /*foreach (var player in looseTokensNumbers)
                    {
                        User user = db.Users.FirstOrDefault(x => x.UserId == player.UserId);
                        result += $"\n<b>·</b><a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>: <b>-{player.BetSum}</b>💵";
                    }*/
                }

                db.SaveChanges();
            }

            return result;
        }
        private static int GetGainSum(RouletteColorEnum? color, int betSum)
        {
            switch (color)
            {
                case RouletteColorEnum.Red:
                    return betSum * 2;
                case RouletteColorEnum.Black:
                    return betSum * 2;
                case RouletteColorEnum.Green:
                    return betSum * 35;
                default:
                    return betSum;
            }
        }
        private static int GetGainSum(int[] choosenNumbers, int betSum)
        {
            return (int)(betSum * RouletteTableStatic.tableSize * ((RouletteTableStatic.tableSize - choosenNumbers.Length) / (double)RouletteTableStatic.tableSize));
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
    }
}
