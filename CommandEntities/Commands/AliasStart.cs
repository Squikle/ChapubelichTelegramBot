using System;
using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Managers;
using ChapubelichBot.Types.Managers.MessagesSender;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChapubelichBot.CommandEntities.Commands
{
    class AliasStart : Command
    {
        public override string Name => AliasGameManager.Name;
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(message.Chat.Id,
                "Эта игра доступна только для чатов! Попробуй написать \"<i>Алиас</i>\" в чате с ботом 😉",
                ParseMode.Html);
        }
    }
}
