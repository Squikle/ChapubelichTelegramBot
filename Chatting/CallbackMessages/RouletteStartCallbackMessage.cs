using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            var gameMessage = await RouletteGameSession.InitializeNew(query.Message, client);
            if (gameMessage != null)
                await client.TryAnswerCallbackQueryAsync(query.Id, "Игра уже запущена!");
        }
    }
} 
