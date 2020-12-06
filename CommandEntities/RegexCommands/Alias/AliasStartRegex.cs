using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Managers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.RegexCommands.Alias
{
    class AliasStartRegex : RegexCommand
    {
        public override string Pattern => @"^\/? *(алиас|угадайка|alias)(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await AliasGameManager.CreateRequestAsync(message);
        }
    }
}
