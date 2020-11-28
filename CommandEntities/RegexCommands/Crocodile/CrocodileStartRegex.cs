using System;
using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Managers.MessagesSender;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.RegexCommands.Crocodile
{
    class CrocodileStartRegex : RegexCommand
    {
        public override string Pattern => @"^\/? *(крокодил|crocodile)(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            throw new NotImplementedException();
            await client.TrySendTextMessageAsync(message.Chat.Id,
                "Игра \"Крокодил\"!" +
                "\nНажмите на кнопку чтобы участвовать в выборе ведущего!",
                replyToMessageId: message.MessageId,
                replyMarkup: InlineKeyboards.CrocodileRegistration);
        }
    }
}
