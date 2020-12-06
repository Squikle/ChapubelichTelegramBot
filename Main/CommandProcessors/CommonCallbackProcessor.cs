using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.CommandProcessors;
using ChapubelichBot.Types.Entities.Groups;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChapubelichBot.Main.CommandProcessors
{
    class CommonCallbackProcessor : CallbackMessageProcessor
    {
        public override async Task<bool> ExecuteAsync(CallbackQuery callbackQuery, ITelegramBotClient client)
        {
            if (GlobalIgnored(callbackQuery))
                return true;
            if (IsResponsiveForChatType(callbackQuery.Message.Chat.Type))
            {
                bool isUserRegistered;
                if (callbackQuery.Message.Chat.Type == ChatType.Group ||
                    callbackQuery.Message.Chat.Type == ChatType.Supergroup)
                {
                    Group groupOfMessage = await UpdateGroupAsync(callbackQuery, client);
                    if (groupOfMessage != null && !groupOfMessage.IsAvailable)
                        return true;

                    isUserRegistered = IsMemberRegistered(callbackQuery.From, groupOfMessage);
                }
                else 
                    isUserRegistered = await IsUserRegisteredAsync(callbackQuery.From);
                return await ProcessCallBackMessageAsync(callbackQuery, isUserRegistered, client);
            }
            return false;
        }
        protected override bool IsResponsiveForChatType(ChatType chatType)
        {
            return true;
        }
        protected override async Task<bool> ProcessCallBackMessageAsync(CallbackQuery callbackQuery, bool isUserRegistered, ITelegramBotClient client)
        {
            var callbackMessages = ChapubelichClient.CallBackMessagesList;
            foreach (var command in callbackMessages)
            {
                if (command.Contains(callbackQuery))
                    if (isUserRegistered)
                    {
                        await command.ExecuteAsync(callbackQuery, client);
                        return true;
                    }
                    else
                    {
                        await SendRegistrationAlertAsync(callbackQuery, client);
                        return true;
                    }
            }

            if (ChapubelichClient.GenderCallbackMessage.Contains(callbackQuery))
            {
                await ChapubelichClient.GenderCallbackMessage.ExecuteAsync(callbackQuery, client);
                return true;
            }

            return false;
        }
    }
}
