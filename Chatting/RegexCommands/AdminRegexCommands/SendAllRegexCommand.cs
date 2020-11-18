using ChapubelichBot.Database;
using ChapubelichBot.Types.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace ChapubelichBot.Chatting.RegexCommands.AdminRegexCommands
{
    class SendAllRegexCommand : RegexCommand
    {

        public override string Pattern => @"^\/SendAll( +([\s\S]+))?$";

        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            string messageText = message.Text ?? message.Caption;

            string sendMessage = Regex.Match(messageText, Pattern).Groups[2].Value;
            List<int> usersToSendId;

            await using (var db = new ChapubelichdbContext())
            {
                usersToSendId = db.Users.Select(x => x.UserId).ToList();
            }

            List<Task> sendingMessages = new List<Task>();

            if (message.Photo != null)
            {
                InputOnlineFile inputFile = new InputOnlineFile(message.Photo[^1].FileId);
                foreach (var userToSendId in usersToSendId)
                {
                    Task sendMessageTask = client.TrySendPhotoAsync(userToSendId, inputFile, sendMessage,
                        Telegram.Bot.Types.Enums.ParseMode.Html);
                    sendingMessages.Add(sendMessageTask);
                }
            }
            else if (message.Audio != null)
            {
                InputOnlineFile inputFile = new InputOnlineFile(message.Audio.FileId);
                InputMedia inputFileThumb = new InputMedia(message.Audio.Thumb.FileId);
                foreach (var userToSendId in usersToSendId)
                {
                    Task sendMessageTask = client.TrySendAudioAsync(userToSendId, inputFile, sendMessage, 
                        Telegram.Bot.Types.Enums.ParseMode.Html, message.Audio.Duration, message.Audio.Performer, 
                        message.Audio.Title, false, 0, null, default, inputFileThumb);
                    sendingMessages.Add(sendMessageTask);
                }
            }
            else if (message.Video != null)
            {
                InputOnlineFile inputFile = new InputOnlineFile(message.Video.FileId);
                InputMedia inputFileThumb = new InputMedia(message.Video.Thumb.FileId);
                foreach (var userToSendId in usersToSendId)
                {
                    Task sendMessageTask = client.TrySendVideoAsync(userToSendId, inputFile, message.Video.Duration, message.Video.Width, 
                        message.Video.Height, sendMessage, Telegram.Bot.Types.Enums.ParseMode.Html, false, false, 0, null, default, inputFileThumb);
                    sendingMessages.Add(sendMessageTask);
                }
            }
            else if (message.Animation != null)
            {
                InputOnlineFile inputFile = new InputOnlineFile(message.Animation.FileId);
                InputMedia inputFileThumb = new InputMedia(message.Animation.Thumb.FileId);
                foreach (var userToSendId in usersToSendId)
                {
                    Task sendMessageTask = client.TrySendAnimationAsync(userToSendId, inputFile, message.Animation.Duration, message.Animation.Width, 
                        message.Animation.Height, inputFileThumb, sendMessage, Telegram.Bot.Types.Enums.ParseMode.Html);
                    sendingMessages.Add(sendMessageTask);
                }
            }
            else if (message.Text != null && !string.IsNullOrEmpty(sendMessage))
            {
                foreach (var userToSendId in usersToSendId)
                {
                    Task sendMessageTask = client.TrySendTextMessageAsync(userToSendId, sendMessage, Telegram.Bot.Types.Enums.ParseMode.Html);
                    sendingMessages.Add(sendMessageTask);
                }
            }

            Task.WaitAll(sendingMessages.ToArray());
        }
    }
}
