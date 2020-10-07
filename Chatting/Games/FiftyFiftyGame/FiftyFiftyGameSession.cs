using Chapubelich.Abstractions;
using Chapubelich.ChapubelichBot.Init;
using Chapubelich.ChapubelichBot.LocalModels;
using Chapubelich.ChapubelichBot.Statics;
using Chapubelich.Database;
using Chapubelich.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Game = Chapubelich.Abstractions.Game;
using User = Chapubelich.Database.Models.User;

namespace Chapubelich.Chatting.Games.FiftyFiftyGame
{
    class FiftyFiftyGameSession
    {
        public long ChatId { get; set; }
        public List<BetToken> BetTokens { get; set; }
        public bool IsActive { get; set; }

        public int ResultNumber { get; set; }
        public Message GameMessage { get; set; }

        public FiftyFiftyGameSession(Message message)
        {
            BetTokens = new List<BetToken>();
            ChatId = message.Chat.Id;
        }

        public async void Start(Message message, ITelegramBotClient client)
        {
            IsActive = true;
            Random rand = new Random();
            ResultNumber = rand.Next(0, 2);

            if (message.From.Id == client.BotId)
                GameMessage = await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Игра в процессе. Ждем ваши ставки: ");
            else
                GameMessage = await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Игра в процессе. Ждем ваши ставки: ",
                    replyToMessageId: message.MessageId);
        }

        public async void Result(ITelegramBotClient client, CallbackQuery query = null, Message message = null)
        {
            using (var db = new ChapubelichdbContext())
            {
                if (null != BetTokens)
                {
                    var winTokens = BetTokens.Where(x => x.Choose == ResultNumber);
                    var looseTokens = BetTokens.Where(x => x.Choose != ResultNumber);

                    string result = "Игра окончена.\nРезультат: ";

                    if (ResultNumber == 0) result += "\U0001F534";
                    else result += "\U000026AB";

                    // Определение победителей
                    if (winTokens.Any())
                    {
                        result += "\n\U0001F3C6<b>Выиграли:</b>";
                        foreach (var token in winTokens)
                        {
                            User user = db.Users.FirstOrDefault(x => x.UserId == token.UserId);

                            result += $"\n<b>·</b><a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>: <b>+{token.BetSum}</b>\U0001F4B0";

                            user.Balance += token.BetSum*2;
                        }
                    }

                    // Определение проигравших
                    if (looseTokens.Any())
                    {
                        result += "\n\U0001F44E<b>Проиграли:</b>";

                        foreach (var player in looseTokens)
                        {
                            User user = db.Users.FirstOrDefault(x => x.UserId == player.UserId);

                            result += $"\n<b>·</b><a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>: <b>-{player.BetSum}</b>\U0001F4B0";
                        }
                    }

                    // Удаление сообщений и отправка результатов
                    if (IsActive)
                    {
                        IsActive = false;

                        InlineKeyboardMarkup playAgainMarkup = new InlineKeyboardMarkup(new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Играть снова", "50/50 PlayAgain")
                        });

                        await client.TryDeleteMessageAsync(
                            GameMessage.Chat.Id,
                            GameMessage.MessageId);

                        await client.TrySendTextMessageAsync(
                            ChatId,
                            result,
                            Telegram.Bot.Types.Enums.ParseMode.Html,
                            replyMarkup: playAgainMarkup);

                        FiftyFiftyGame.GameSessions.Remove(this);

                        db.SaveChanges();
                    }
                }
            }
        }
    }
}
