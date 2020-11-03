using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using ChapubelichBot.Database;
using ChapubelichBot.Types.Extensions;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = ChapubelichBot.Database.Models.User;
using ChapubelichBot.Types.Enums;
using ChapubelichBot.Types.Games.RouletteGame;
using System.Threading.Tasks;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteColorBetRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/?(\d{1,4}) *(к(расный)?|ч(ерный)?|з(еленый)?|r(ed)?|b(lack)?|g(reen)?) *(го|ролл|погнали|крути|roll|go)?(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            var gameSession = RouletteTableStatic.GetGameSessionOrNull(message.Chat.Id);
            if (gameSession != null)
                await gameSession.BetColor(message, Pattern, client);
        }
    }
}
