using Chapubelich.ChapubelichBot.Init;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Chapubelich.Abstractions
{
    public abstract class Command
    {
        public abstract string Name { get; }
        public abstract void Execute(Message message, ITelegramBotClient client);
        public bool Contains(Message message, bool privateChat)
        {
            if (privateChat)
                return message.Text.Contains(Name);

            return message.Text.Contains(Name) && message.Text.Contains(AppSettings.Name);
        }
    }
}
