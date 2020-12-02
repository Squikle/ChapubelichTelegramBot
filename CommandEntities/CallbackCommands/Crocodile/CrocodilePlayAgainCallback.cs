using System.Collections.Generic;
using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Managers;
using ChapubelichBot.Types.Managers.MessagesSender;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.CallbackCommands.Crocodile
{
    class CrocodilePlayAgainCallback : CallbackCommand
    {
        public override List<string> IncludingData => new List<string> { "crocodilePlayAgain" };
        public override async Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
            await client.TryEditMessageReplyMarkupAsync(query.Message.Chat.Id, query.Message.MessageId);
            await CrocodileGameManager.CreateRequestAsync(query.Message);
        }
    }
}
