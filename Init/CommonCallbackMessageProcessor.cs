using System.Threading.Tasks;
using ChapubelichBot.Database.Models;
using ChapubelichBot.Types.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChapubelichBot.Init
{
    class CommonCallbackMessageProcessor : CallbackMessageProcessor
    {
        public override async Task<bool> Execute(CallbackQuery callbackQuery, ITelegramBotClient client)
        {
            if (GlobalIgnored(callbackQuery))
                return true;
            if (IsResponsiveForChatType(callbackQuery.Message.Chat.Type))
            {
                bool isUserRegistered;
                if (callbackQuery.Message.Chat.Type == ChatType.Group ||
                    callbackQuery.Message.Chat.Type == ChatType.Supergroup)
                {
                    Group groupOfMessage = await UpdateGroup(callbackQuery.Message, client);
                    if (groupOfMessage != null && !groupOfMessage.IsAvailable)
                        return true;

                    isUserRegistered = IsMemberRegistered(callbackQuery.From, groupOfMessage);
                }
                else 
                    isUserRegistered = IsUserRegistered(callbackQuery.From);
                return await ProcessCallBackMessage(callbackQuery, isUserRegistered, client);
            }
            return false;
        }
        protected override bool IsResponsiveForChatType(ChatType chatType)
        {
            return true;
        }
        protected override async Task<bool> ProcessCallBackMessage(CallbackQuery callbackQuery, bool isUserRegistered, ITelegramBotClient client)
        {
            var callbackMessages = Bot.CallBackMessagesList;
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

            if (Bot.GenderCallbackMessage.Contains(callbackQuery))
            {
                await Bot.GenderCallbackMessage.ExecuteAsync(callbackQuery, client);
                return true;
            }

            return false;
        }
    }
}
