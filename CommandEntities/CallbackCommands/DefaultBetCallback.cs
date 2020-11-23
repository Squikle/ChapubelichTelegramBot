using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Managers;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = ChapubelichBot.Types.Entities.User;

namespace ChapubelichBot.CommandEntities.CallbackCommands
{
    class DefaultBetChangeCallbackMessage : CallbackCommand
    {
        public override List<string> IncludingData => new List<string> { "DefaultBet25", "DefaultBet50", "DefaultBet100", "DefaultBet500" };
        public override async Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
            await using var db = new ChapubelichdbContext();
            short defaultBet = 50;

            switch (query.Data)
            {
                case "DefaultBet25":
                    defaultBet = 25;
                    break;
                case "DefaultBet50":
                    defaultBet = 50;
                    break;
                case "DefaultBet100":
                    defaultBet = 100;
                    break;
                case "DefaultBet500":
                    defaultBet = 500;
                    break;
            }

            User senderUser = db.Users.FirstOrDefault(x => x.UserId == query.From.Id);
            if (senderUser == null)
                return;

            senderUser.DefaultBet = defaultBet;
            db.SaveChanges();
            Task deletingMessage = client.TryDeleteMessageAsync(
                query.Message.Chat.Id,
                query.Message.MessageId);
            await client.TrySendTextMessageAsync(
                query.Message.Chat.Id,
                "Настройки успешно сохранены!",
                replyMarkup: ReplyKeyboards.SettingsMarkup
            );
            await deletingMessage;
        }
    }
}
