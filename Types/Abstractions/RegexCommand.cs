using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Types.Abstractions
{
    public abstract class RegexCommand
    {
        public abstract string Pattern { get; }
        public abstract void Execute(Message message, ITelegramBotClient client);
        /*public async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await Task.Run(() =>
            {
                Execute(message, client);
            });
        }*/
        public bool Contains(string text)
        {
            return Regex.IsMatch(text, Pattern, RegexOptions.IgnoreCase);
        }
    }
}
