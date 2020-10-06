using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Chapubelich.Abstractions
{
    public abstract class RegexCommand
    {
        public abstract string Pattern { get; }
        public abstract void Execute(Message message, ITelegramBotClient client);
        public bool Contains(string text)
        {
            return Regex.IsMatch(text, Pattern, RegexOptions.IgnoreCase);
        }
    }
}
