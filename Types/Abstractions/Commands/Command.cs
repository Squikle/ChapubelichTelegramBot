using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Types.Abstractions.Commands
{
    public abstract class Command
    {
        public abstract string Name { get; }
        public abstract Task ExecuteAsync(Message message, ITelegramBotClient client);
        public bool Contains(string text)
        {
            return text.Contains(Name);
        }
    }
}
