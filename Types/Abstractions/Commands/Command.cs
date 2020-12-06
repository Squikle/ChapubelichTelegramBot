using System.Threading.Tasks;
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
