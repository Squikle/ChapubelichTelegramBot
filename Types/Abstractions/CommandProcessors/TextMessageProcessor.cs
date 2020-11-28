using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Managers.MessagesSender;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChapubelichBot.Types.Abstractions.CommandProcessors
{
    public abstract class TextMessageProcessor : MessageProcessor
    {
        public abstract Task<bool> ExecuteAsync(Message message, ITelegramBotClient client);
        protected static async Task SendRegistrationAlertAsync(Message message, ITelegramBotClient client)
        {
            if (message.Chat.Type == ChatType.Private)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Упс, кажется тебя нет в базе данных. Пожалуйста, пройди процесс регистрации: ",
                    replyToMessageId: message.MessageId);
                await ChapubelichClient.RegistrationCommand.ExecuteAsync(message, client);
            }
            else if (message.Chat.Type == ChatType.Group ||
                     message.Chat.Type == ChatType.Supergroup)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Кажется, вы не зарегистрированы. Для регистрации обратитесь к боту в личные сообщения 💌",
                    replyToMessageId: message.MessageId
                );
            }
        }
        protected bool GlobalIgnored(Message message)
        {
            return message.ForwardFrom != null;
        }
    }
}
