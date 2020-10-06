using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Chapubelich.Database;
using Telegram.Bot.Types.ReplyMarkups;
using Chapubelich.Abstractions;
using Chapubelich.Extensions;

namespace Chapubelich.Chatting.CallbackMessages
{
    class GenderCallbackMessage : CallBackMessage
    {
        public override List<string> IncludingData => new List<string> { "Male", "Female" };

        public override async void Execute(CallbackQuery query, ITelegramBotClient client)
        {
            bool userGender = false;

            switch (query.Data)
            {
                case "Male":
                    userGender = true;
                    break;
                case "Female":
                    userGender = false;
                    break;
            }

            Database.Models.User user = new Database.Models.User()
            {
                Gender = userGender,
                Username = query.From.Username,
                UserId = query.From.Id,
                FirstName = query.From.FirstName
            };

            var keyboard = new[] { 
                new KeyboardButton("\U0001F4B0 Баланс"),
                new KeyboardButton("\U0001F579 Игры")
            };
            var markup = new ReplyKeyboardMarkup(keyboard, resizeKeyboard: false, oneTimeKeyboard: false);

            using (var db = new ChapubelichdbContext())
            {
                if (!db.Users.Any(x => x.UserId == user.UserId))
                {
                    db.Users.Add(user);
                    db.SaveChanges();
                    await client.TrySendTextMessageAsync(
                        user.UserId,
                        "Вы были успешно зарегестрированы!",
                        replyMarkup: markup
                        );
                }
                else 
                    await client.TrySendTextMessageAsync(
                        user.UserId,
                        "Вы уже зарегестрированы!",
                        replyMarkup: markup
                        );
            }

            await client.TryDeleteMessageAsync(query.Message.Chat.Id, query.Message.MessageId);

            return;
        }
    }
}
