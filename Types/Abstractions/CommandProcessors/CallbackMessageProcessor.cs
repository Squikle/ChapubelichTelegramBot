using System.Threading.Tasks;
using ChapubelichBot.Types.Managers.MessagesSender;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChapubelichBot.Types.Abstractions.CommandProcessors
{
    public abstract class CallbackMessageProcessor : MessageProcessor
    {
        public abstract Task<bool> ExecuteAsync(CallbackQuery callbackQuery, ITelegramBotClient client);
        protected abstract bool IsResponsiveForChatType(ChatType chatType);
        protected abstract Task<bool> ProcessCallBackMessageAsync(CallbackQuery callBackQuery, bool isUserRegistered, ITelegramBotClient client);
        protected static async Task SendRegistrationAlertAsync(CallbackQuery callbackQuery, ITelegramBotClient client)
        {
            await client.TryAnswerCallbackQueryAsync(
                callbackQuery.Id,
                "Пожалуйста, пройдите процесс регистрации.",
                showAlert: true);
        }
        protected bool GlobalIgnored(CallbackQuery callbackQuery)
        {
            return callbackQuery.Data == null;
        }
    }
}
