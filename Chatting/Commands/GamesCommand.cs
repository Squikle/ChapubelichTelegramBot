using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.Commands
{
    class GamesCommand : Command
    {
        public override string Name => "\U0001F579 Игры";

        public override async void Execute(Message message, ITelegramBotClient client)
        {          
            await client.TrySendTextMessageAsync(
                        message.Chat.Id,
                        "\U0001F579 Список доступных игр!",
                        replyMarkup: ReplyKeyboardsStatic.GameMarkup,
                        replyToMessageId: message.MessageId);
        }
    }
}
