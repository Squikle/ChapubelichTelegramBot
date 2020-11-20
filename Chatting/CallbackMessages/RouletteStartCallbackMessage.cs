using ChapubelichBot.Types.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChapubelichBot.Database.Models;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using ChapubelichBot.Types.Games.RouletteGame;

namespace ChapubelichBot.Chatting.CallbackMessages
{
    class RouletteStartCallbackMessage : CallBackMessage
    {
        public override List<string> IncludingData => new List<string> { "roulettePlayAgain" };
        public override async Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
            await client.TryEditMessageReplyMarkupAsync(query.Message.Chat.Id, query.Message.MessageId);
            RouletteGameSession gameSession = RouletteGameManager.GetGameSessionOrNull(query.Message.MessageId);
            if (gameSession != null)
            {
                await client.TrySendTextMessageAsync(query.Message.Chat.Id,
                    "Игра уже запущена!",
                    replyToMessageId: gameSession.GameMessageId);
            }
            else
            {
                await RouletteGameManager.StartAsync(query.Message);
            }
        }
    }
} 
