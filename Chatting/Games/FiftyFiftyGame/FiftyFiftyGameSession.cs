﻿using Chapubelich.Abstractions;
using Chapubelich.ChapubelichBot.Init;
using Chapubelich.ChapubelichBot.LocalModels;
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
    class FiftyFiftyGameSession : GameSession
    {
        public override Game CurrentGame { get; set; }
        public override long ChatId { get; set; }
        public override List<BetToken> BetTokens { get; set; }
        public override bool IsActive { get; set; }

        public int ResultNumber { get; set; }
        public override Message GameMessage { get; set; }

        public FiftyFiftyGameSession(Message message) : base(message)
        {
            CurrentGame = Bot.BotGamesList.First(x => x.Name == "\U0001F3B0 50/50");
        }

        public override async void Start(Message message, ITelegramBotClient client)
        {
            IsActive = true;
            Random rand = new Random();
            ResultNumber = rand.Next(0, 2);
            GameMessage = await client.TrySendTextMessageAsync(
                message.Chat.Id,
                "Игра в процессе. Ждем ваши ставки: ");
        }

        public override async void Result(ITelegramBotClient client, CallbackQuery query = null, Message message = null)
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

                        if (null != GameMessage)
                            await client.TryDeleteMessageAsync(
                                GameMessage.Chat.Id,
                                GameMessage.MessageId);

                        await client.TrySendTextMessageAsync(
                            ChatId,
                            result,
                            Telegram.Bot.Types.Enums.ParseMode.Html);

                        CurrentGame.GameSessions.Remove(this);

                        db.SaveChanges();
                    }
                }
            }
        }
    }
}
