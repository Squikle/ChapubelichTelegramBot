using System.Collections.Generic;
using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Managers;
using ChapubelichBot.Types.Managers.MessagesSender;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.CallbackCommands.Alias
{
    class AliasPlayAgainCallback : CallbackCommand
    {
        public override List<string> IncludingData => new List<string> { "aliasPlayAgain" };
        public override async Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
            await client.TryEditMessageReplyMarkupAsync(query.Message.Chat.Id, query.Message.MessageId);
            await AliasGameManager.CreateRequestAsync(query.Message);
        }
    }
}
