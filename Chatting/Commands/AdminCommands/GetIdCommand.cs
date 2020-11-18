﻿using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.Commands.AdminCommands
{
    class GetIdCommand : Command
    {
        public override string Name => "/getid";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            Message answer = await client.TrySendTextMessageAsync(message.Chat.Id, "Получаю пользователя...", replyToMessageId: message.MessageId+1);

            if (answer == null)
            {
                await client.TrySendTextMessageAsync(message.Chat.Id, "Не удалось получить пользователя.", replyToMessageId: message.MessageId);
                return;
            }
                
            if (answer.ReplyToMessage?.ForwardFrom != null)
                await client.TryEditMessageAsync(message.Chat.Id, answer.MessageId,
                    $"{answer.ReplyToMessage.ForwardFrom.FirstName} : {answer.ReplyToMessage.ForwardFrom.Id}");
        }
    }
}
