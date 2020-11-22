using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.CommandProcessors;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChapubelichBot.Main.CommandProcessors
{
    class AdminMessageProcessor : MessageProcessor
    {
        public override async Task<bool> Execute(Message message, ITelegramBotClient client)
        {
            if (GlobalIgnored(message))
                return true;
            bool isUserRegistered = IsUserRegistered(message.From);
            return await ProcessMessage(message, isUserRegistered, client);
        }
        protected async Task<bool> ProcessMessage(Message message, bool isUserRegistered, ITelegramBotClient client)
        {
            if (!isUserRegistered)
                return true;
            if (message.From.Id == 243857110)
            {
                foreach (var adminRegexCommand in ChapubelichClient.BotAdminRegexCommandsList)
                {
                    if (message.Text != null && adminRegexCommand.Contains(message.Text)
                        || message.Caption != null && adminRegexCommand.Contains(message.Caption))
                    {
                        await adminRegexCommand.ExecuteAsync(message, client);
                        return true;
                    }
                }

                if (message.Type == MessageType.Text)
                {
                    foreach (var adminCommand in ChapubelichClient.BotAdminCommandsList)
                    {
                        if (adminCommand.Contains(message.Text, privateChat: true))
                        {
                            await adminCommand.ExecuteAsync(message, client);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
