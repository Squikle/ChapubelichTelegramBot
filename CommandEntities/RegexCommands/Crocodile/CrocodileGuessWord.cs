using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Managers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.RegexCommands.Crocodile
{
    class CrocodileGuessWord : RegexCommand
    {
        public override string Pattern => @"^\/?\..*$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await CrocodileGameManager.GuessTheWordRequestAsync(message);
        }
    }
}
