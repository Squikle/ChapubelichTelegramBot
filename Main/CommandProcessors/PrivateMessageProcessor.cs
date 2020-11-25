using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.CommandProcessors;
using ChapubelichBot.Types.Managers;
using ChapubelichBot.Types.Managers.MessagesSender;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChapubelichBot.Main.CommandProcessors
{
    class PrivateMessageProcessor : TextMessageProcessor
    {
        public override async Task<bool> ExecuteAsync(Message message, ITelegramBotClient client)
        {
            if (GlobalIgnored(message))
                return true;
            if (IsResponsiveForMessageType(message.Type) && IsResponsiveForChatType(message.Chat.Type))
            {
                bool isUserRegistered = await IsUserRegisteredAsync(message.From);
                return await ProcessMessageAsync(message, isUserRegistered, client);
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
        protected async Task<bool> ProcessMessageAsync(Message message, bool isUserRegistered, ITelegramBotClient client)
        {
            bool repeatedRegisterRequest = false;
            if (isUserRegistered)
            {
                foreach (var privateCommand in ChapubelichClient.BotPrivateCommandsList)
                    if (privateCommand.Contains(message.Text, privateChat: true))
                    {
                        await privateCommand.ExecuteAsync(message, client);
                        return true;
                    }

                foreach (var regexCommand in ChapubelichClient.BotRegexCommandsList)
                    if (regexCommand.Contains(message.Text))
                    {
                        await regexCommand.ExecuteAsync(message, client);
                        return true;
                    }
            }

            if (ChapubelichClient.StartCommand.Contains(message.Text, privateChat: true))
            {
                if (!isUserRegistered)
                {
                    await ChapubelichClient.StartCommand.ExecuteAsync(message, client);
                    return true;
                }

                repeatedRegisterRequest = true;
            }
            else if (ChapubelichClient.RegistrationCommand.Contains(message.Text, privateChat: true))
            {
                if (!isUserRegistered)
                {
                    await ChapubelichClient.RegistrationCommand.ExecuteAsync(message, client);
                    return true;
                }

                repeatedRegisterRequest = true;
            }

            if (repeatedRegisterRequest)
            {
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    "Ты уже зарегестрирован 👍",
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
                "Я тебя не понял :С Воспользуйся меню. (Если его нет - нажми на соответствующую кнопку на поле ввода 👇)",
                replyMarkup: ReplyKeyboards.MainMarkup,
                replyToMessageId: message.MessageId);
            return true;
        }
    }
}
