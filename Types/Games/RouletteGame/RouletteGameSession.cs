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
        public bool Rolling { get; set; }
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
            Rolling = false;

            ResultNumber = RouletteColorStatic.GetRandomResultNumber();

            int replyId = message.From.Id == client.BotId ? 0 : message.MessageId;
            GameMessage = await client.TrySendTextMessageAsync(
                message.Chat.Id,
                "Игра запущена. Ждем ваши ставки: ",
                replyToMessageId: replyId);
        }
        public async void Result(ITelegramBotClient client, Message startMessage = null)
        {
            if (Rolling)
                return;

            await client.SendAnimationAsync(ChatId, GetRandomAnimationLink(), duration: 3000, disableNotification: true);
            Message resultMessage = await client.TrySendTextMessageAsync(ChatId, "Крутим барабан...", replyToMessageId: startMessage.MessageId);

            Rolling = true;
            // Удаление сообщений и отправка результатов
            if (!Resulting)
            {
                Resulting = true;
                string result = GetResultMessage();

                Thread.Sleep(3000);

                await client.TryDeleteMessageAsync(
                    GameMessage.Chat.Id,
                    GameMessage.MessageId);

                await client.TryEditMessageAsync(
                    ChatId,
                    resultMessage.MessageId,
                    result,
                    Telegram.Bot.Types.Enums.ParseMode.Html,
                    replyMarkup: InlineKeyboardsStatic.roulettePlayAgainMarkup);

                RouletteGameStatic.GameSessions.Remove(this);
            }
        }

        private string GetResultMessage()
        {
            string result = "Игра окончена.\nРезультат: ";
            result += RouletteColorStatic.GetEmojiByColor(RouletteColorStatic.GetColorByNumber(ResultNumber));
            var winTokens = BetTokens.Where(x => x.ColorChoose == RouletteColorStatic.GetColorByNumber(ResultNumber));
            var groupendLooseTokens = new List<RouletteBetToken>();
            foreach (var el in BetTokens.Except(winTokens))
            {
                var token = groupendLooseTokens.FirstOrDefault(x => x.UserId == el.UserId);
                    if (token != null)
                    token.BetSum += el.BetSum;
                else groupendLooseTokens.Add(el);
            }

            using (var db = new ChapubelichdbContext())
            {
                // Определение победителей
                if (winTokens.Any())
                {
                    result += "\n🏆<b>Выиграли:</b>";
                    foreach (var token in winTokens)
                    {
                        int gainSum = GetGainSum(token.ColorChoose, token.BetSum);
                        User user = db.Users.FirstOrDefault(x => x.UserId == token.UserId);
                        result += $"\n<b>·</b><a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>: <b>+{gainSum}</b>💵";
                        user.Balance += gainSum;
                    }
                }
                // Определение проигравших
                if (groupendLooseTokens.Any())
                {
                    result += "\n\U0001F614<b>Проиграли:</b>";
                    foreach (var player in groupendLooseTokens)
                    {
                        User user = db.Users.FirstOrDefault(x => x.UserId == player.UserId);
                        result += $"\n<b>·</b><a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>: <b>-{player.BetSum}</b>💵";
                    }
                }

                db.SaveChanges();
            }

            return result;
        }
        private static int GetGainSum(RouletteColorEnum color, int betSum)
        {
            switch (color)
            {
                case RouletteColorEnum.Red:
                    return betSum;
                case RouletteColorEnum.Black:
                    return betSum;
                case RouletteColorEnum.Green:
                    return betSum * 35;
                default:
                    return betSum;
            }
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
