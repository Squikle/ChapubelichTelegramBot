using ChapubelichBot.Types.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using ChapubelichBot.Types.Games.RouletteGame;
using ChapubelichBot.Types.Statics;

namespace ChapubelichBot.Chatting.CallbackMessages
{
    class RouletteStartCallbackMessage : CallBackMessage
    {
        public override List<string> IncludingData => new List<string> { "roulettePlayAgain" };
        public override async Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
            await client.TryEditMessageReplyMarkupAsync(query.Message.Chat.Id, query.Message.MessageId);
            RouletteGameSession gameSession = RouletteGame.GetGameSessionOrNull(query.Message.MessageId);
            if (gameSession == null)
                await RouletteGame.InitializeNew(query.Message, client);
            else
            {
                await client.TrySendTextMessageAsync(query.Message.Chat.Id,
                    "Игра уже запущена!",
                    replyToMessageId: gameSession.GameMessage.MessageId);
            }
        }
    }
} 
