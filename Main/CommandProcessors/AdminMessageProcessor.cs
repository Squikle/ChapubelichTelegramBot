using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.CommandProcessors;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChapubelichBot.Main.CommandProcessors
{
    class AdminMessageProcessor : TextMessageProcessor
    {
        public override async Task<bool> ExecuteAsync(Message message, ITelegramBotClient client)
        {
            if (GlobalIgnored(message))
                return true;
            bool isUserRegistered = await IsUserRegisteredAsync(message.From);
            return await ProcessMessageAsync(message, isUserRegistered, client);
        }
        protected async Task<bool> ProcessMessageAsync(Message message, bool isUserRegistered, ITelegramBotClient client)
        {
            if (!isUserRegistered)
                return false;
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
                        if (adminCommand.Contains(message.Text))
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
