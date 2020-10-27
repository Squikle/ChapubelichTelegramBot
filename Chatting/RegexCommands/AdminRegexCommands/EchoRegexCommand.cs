using ChapubelichBot.Database;
using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace ChapubelichBot.Chatting.RegexCommands.AdminRegexCommands
{
    class EchoRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/echo( +([\s\S]+))?$";
        
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            string messageText = message.Text == null ? message.Caption : message.Text;
            string sendMessage = Regex.Match(messageText, Pattern).Groups[2].Value;

            if (message.Photo != null)
            {
                InputOnlineFile inputFile = new InputOnlineFile(message.Photo[message.Photo.Length - 1].FileId);
                await client.TrySendPhotoAsync(message.Chat.Id, inputFile, sendMessage, Telegram.Bot.Types.Enums.ParseMode.Html, false, 0, null, default(System.Threading.CancellationToken));
            }
            else if (message.Audio != null)
            {
                InputOnlineFile inputFile = new InputOnlineFile(message.Audio.FileId);
                InputMedia inputFileThumb = new InputMedia(message.Audio.Thumb.FileId);
                await client.TrySendAudioAsync(message.Chat.Id, inputFile, sendMessage, Telegram.Bot.Types.Enums.ParseMode.Html, message.Audio.Duration, message.Audio.Performer, message.Audio.Title, false, 0, null, default(System.Threading.CancellationToken), inputFileThumb);
            }
            else if (message.Video != null)
            {
                InputOnlineFile inputFile = new InputOnlineFile(message.Video.FileId);
                InputMedia inputFileThumb = new InputMedia(message.Video.Thumb.FileId);
                await client.TrySendVideoAsync(message.Chat.Id, inputFile, message.Video.Duration, message.Video.Width, message.Video.Height, sendMessage, Telegram.Bot.Types.Enums.ParseMode.Html, false, false, 0, null, default(System.Threading.CancellationToken), inputFileThumb);
            }
            else if (message.Animation != null)
            {
                InputOnlineFile inputFile = new InputOnlineFile(message.Animation.FileId);
                InputMedia inputFileThumb = new InputMedia(message.Animation.Thumb.FileId);
                await client.TrySendAnimationAsync(message.Chat.Id, inputFile, message.Animation.Duration, message.Animation.Width, message.Animation.Height, inputFileThumb, sendMessage, Telegram.Bot.Types.Enums.ParseMode.Html, false, 0, null, default(System.Threading.CancellationToken));
            }
            else if (message.Text != null && !string.IsNullOrEmpty(sendMessage))
            {
                await client.TrySendTextMessageAsync(message.Chat.Id, sendMessage, Telegram.Bot.Types.Enums.ParseMode.Html, false, false, 0, null, default(System.Threading.CancellationToken));
            }
                /*else if (message.Poll != null)
                {
                    Telegram.Bot.Types.Enums.PollType pollType = message.Poll.Type == "regular" ? Telegram.Bot.Types.Enums.PollType.Regular : Telegram.Bot.Types.Enums.PollType.Quiz;
                    await client.TrySendPollAsync(userToSend.UserId, message.Poll.Question, message.Poll.Options.Select(x => x.Text), false, 0, null, default(System.Threading.CancellationToken), message.Poll.IsAnonymous, pollType, message.Poll.AllowsMultipleAnswers, message.Poll.CorrectOptionId, message.Poll.IsClosed, message.Poll.Explanation, Telegram.Bot.Types.Enums.ParseMode.Html, message.Poll.OpenPeriod, message.Poll.CloseDate);
                }*/
            }
    }
}
