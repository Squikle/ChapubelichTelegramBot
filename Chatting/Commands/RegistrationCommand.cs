using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.Commands
{
    public class RegistrationCommand : Command
    {
        public override string Name => "\U0001F511 Регистрация";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(message.Chat.Id,
            "Пожалуйста, укажите ваш гендер:",
            replyMarkup: InlineKeyboardsStatic.genderChooseMarkup);
        }
    }
}
