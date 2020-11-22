using System.Threading.Tasks;
using ChapubelichBot.Database.Models;
using ChapubelichBot.Types.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChapubelichBot.Init
{
    class GroupMessageProcessor : MessageProcessor
    {
        public override async Task<bool> Execute(Message message, ITelegramBotClient client)
        {
            if (GlobalIgnored(message))
                return true;
            if (IsResponsiveForMessageType(message.Type) && IsResponsiveForChatType(message.Chat.Type))
            {
                Group group = await UpdateGroup(message, client);
                if (group == null)
                    return true;
                bool isUserRegistered = IsMemberRegistered(message.From, group);
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
            return chatType == ChatType.Group || chatType == ChatType.Supergroup;
        }

        protected async Task<bool> ProcessMessage(Message message, bool isUserRegistered, ITelegramBotClient client)
        {
            if (message.Type != MessageType.Text)
                return false;

            foreach (var groupCommand in Bot.BotGroupRegexCommandsList)
            {
                if (groupCommand.Contains(message.Text))
                {
                    if (isUserRegistered)
                        await groupCommand.ExecuteAsync(message, client);
                    else
                        await SendRegistrationAlertAsync(message, client);

                    return true;
                }
            }

            foreach (var regexCommand in Bot.BotRegexCommandsList)
            {
                if (regexCommand.Contains(message.Text))
                {
                    if (isUserRegistered)
                        await regexCommand.ExecuteAsync(message, client);
                    else
                        await SendRegistrationAlertAsync(message, client);

                    return true;
                }
            }

            return false;
        }

    }
}
