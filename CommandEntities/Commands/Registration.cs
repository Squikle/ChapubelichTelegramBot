﻿using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.Commands
{
    public class Registration : Command
    {
        public override string Name => "🔑 Регистрация";

        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(message.Chat.Id,
            "Пожалуйста, укажите свой пол:",
            replyMarkup: InlineKeyboards.GenderChooseMarkup);
        }
    }
}