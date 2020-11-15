using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using ChapubelichBot.Types.Extensions;

namespace ChapubelichBot.Chatting.Commands
{
    class GamesCommand : Command
    {
        public override string Name => "🕹 Игры";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {          
            await client.TrySendTextMessageAsync(
                        message.Chat.Id,
                        "\U0001F579 Список доступных игр!",
                        replyMarkup: ReplyKeyboards.GameMarkup,
                        replyToMessageId: message.MessageId);
        }
    }
}
