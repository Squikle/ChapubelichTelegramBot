using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.CommandProcessors;
using ChapubelichBot.Types.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChapubelichBot.Main.CommandProcessors
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

            foreach (var groupCommand in ChapubelichClient.BotGroupRegexCommandsList)
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

            foreach (var regexCommand in ChapubelichClient.BotRegexCommandsList)
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
