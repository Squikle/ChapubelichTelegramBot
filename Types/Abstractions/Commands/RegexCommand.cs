using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Types.Abstractions.Commands
{
    public abstract class RegexCommand
    {
        public abstract string Pattern { get; }
        public abstract Task ExecuteAsync(Message message, ITelegramBotClient client);
        public bool Contains(string text)
        {
            return Regex.IsMatch(text, Pattern, RegexOptions.IgnoreCase);
        }
    }
}
