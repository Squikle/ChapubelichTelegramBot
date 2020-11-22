using System.Linq;
using System.Threading.Tasks;
using ChapubelichBot.Database;
using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChapubelichBot.Init
{
    class PrivateMessageProcessor : MessageProcessor
    {
        public override async Task<bool> Execute(Message message, ITelegramBotClient client)
        {
            if (GlobalIgnored(message))
                return true;
            if (IsResponsiveForMessageType(message.Type) && IsResponsiveForChatType(message.Chat.Type))
            {
                bool isUserRegistered = IsUserRegistered(message.From);
                return await ProcessMessage(message, isUserRegistered, client);
            }
            return false;
        }
        protected bool IsResponsiveForMessageType(MessageType messageType)
        {
            return messageType == MessageType.Text;
        }
        protected bool IsResponsiveForChatType(ChatType chatType)
        {
            return chatType == ChatType.Private;
        }
        protected async Task<bool> ProcessMessage(Message message, bool isUserRegistered, ITelegramBotClient client)
        {
            bool repeatedRegisterRequest = false;
            if (isUserRegistered)
            {
                foreach (var privateCommand in Bot.BotPrivateCommandsList)
                    if (privateCommand.Contains(message.Text, privateChat: true))
                    {
                        await privateCommand.ExecuteAsync(message, client);
                        return true;
                    }

                foreach (var regexCommand in Bot.BotRegexCommandsList)
                    if (regexCommand.Contains(message.Text))
                    {
                        await regexCommand.ExecuteAsync(message, client);
                        return true;
                    }
            }

            if (Bot.StartCommand.Contains(message.Text, privateChat: true))
            {
                if (!isUserRegistered)
                {
                    await Bot.StartCommand.ExecuteAsync(message, client);
                    return true;
                }

                repeatedRegisterRequest = true;
            }
            else if (Bot.RegistrationCommand.Contains(message.Text, privateChat: true))
            {
                if (!isUserRegistered)
                {
                    await Bot.RegistrationCommand.ExecuteAsync(message, client);
                    return true;
                }

                repeatedRegisterRequest = true;
            }

            if (repeatedRegisterRequest)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Ты уже зарегестрирован👍",
                    replyMarkup: ReplyKeyboards.MainMarkup);

                return true;
            }

            if (!isUserRegistered)
            {
                await SendRegistrationAlertAsync(message, client);
                return true;
            }

            await client.TrySendTextMessageAsync(
                message.Chat.Id,
                "Я тебя не понял :С Воспользуйся меню. (Если его нет - нажми на соответствующую кнопку на поле ввода👇)",
                replyMarkup: ReplyKeyboards.MainMarkup,
                replyToMessageId: message.MessageId);
            return true;
        }
    }
}
