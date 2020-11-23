using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Types.Entities.Messages
{
    public interface IDelayedMessage
    {
        Task<Message> Send(ITelegramBotClient client);
    } 
}
