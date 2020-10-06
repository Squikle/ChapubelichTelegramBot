using Chapubelich.ChapubelichBot.Init;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Chapubelich.Abstractions
{
    public abstract class Command
    {
        public abstract string Name { get; }
        public abstract void Execute(Message message, ITelegramBotClient client);
        public bool Contains(string text, bool privateChat)
        {
            if (privateChat)
                return text.Contains(Name);

            return text.Contains(Name) && text.Contains(AppSettings.Name);
        }
    }
}
