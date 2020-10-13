using ChapubelichBot.Init;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Types.Abstractions
{
    public abstract class Command
    {
        public abstract string Name { get; }
        public abstract Task ExecuteAsync(Message message, ITelegramBotClient client);
        public bool Contains(string text, bool privateChat)
        {
            if (privateChat)
                return text.Contains(Name);

            return text.Contains(Name) && text.Contains(AppSettings.Name);
        }
    }
}
