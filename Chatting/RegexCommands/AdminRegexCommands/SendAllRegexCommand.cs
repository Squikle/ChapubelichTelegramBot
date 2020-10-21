using ChapubelichBot.Database;
using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
            string messageText = message.Text == null ? message.Caption : message.Text;

            string sendMessage = Regex.Match(messageText, Pattern).Groups[2].Value;
            List<int> usersToSendId = new List<int>();

            using (var db = new ChapubelichdbContext())
            {
                usersToSendId = db.Users.Where(x => x.IsAvailable).Select(x => x.UserId).ToList();
            }

            if (message.Photo != null)
            {
                InputOnlineFile inputFile = new InputOnlineFile(message.Photo[message.Photo.Length - 1].FileId);
                foreach (var userToSendId in usersToSendId)
                {
                    await client.TrySendPhotoAsync(userToSendId, inputFile, sendMessage, Telegram.Bot.Types.Enums.ParseMode.Html, false, 0, null, default(System.Threading.CancellationToken));
                }
            }
            else if (message.Audio != null)
            {
                InputOnlineFile inputFile = new InputOnlineFile(message.Audio.FileId);
                InputMedia inputFileThumb = new InputMedia(message.Audio.Thumb.FileId);
                foreach (var userToSendId in usersToSendId)
                {
                    await client.TrySendAudioAsync(userToSendId, inputFile, sendMessage, Telegram.Bot.Types.Enums.ParseMode.Html, message.Audio.Duration, message.Audio.Performer, message.Audio.Title, false, 0, null, default(System.Threading.CancellationToken), inputFileThumb);
                }
            }
            else if (message.Video != null)
            {
                InputOnlineFile inputFile = new InputOnlineFile(message.Video.FileId);
                InputMedia inputFileThumb = new InputMedia(message.Video.Thumb.FileId);
                foreach (var userToSendId in usersToSendId)
                {
                    await client.TrySendVideoAsync(userToSendId, inputFile, message.Video.Duration, message.Video.Width, message.Video.Height, sendMessage, Telegram.Bot.Types.Enums.ParseMode.Html, false, false, 0, null, default(System.Threading.CancellationToken), inputFileThumb);
                }
            }
            else if (message.Animation != null)
            {
                InputOnlineFile inputFile = new InputOnlineFile(message.Animation.FileId);
                InputMedia inputFileThumb = new InputMedia(message.Animation.Thumb.FileId);
                foreach (var userToSendId in usersToSendId)
                {
                    await client.TrySendAnimationAsync(userToSendId, inputFile, message.Animation.Duration, message.Animation.Width, message.Animation.Height, inputFileThumb, sendMessage, Telegram.Bot.Types.Enums.ParseMode.Html, false, 0, null, default(System.Threading.CancellationToken));
                }
            }
            else if (message.Text != null && !string.IsNullOrEmpty(sendMessage))
            {
                foreach (var userToSendId in usersToSendId)
                {
                    await client.TrySendTextMessageAsync(userToSendId, sendMessage, Telegram.Bot.Types.Enums.ParseMode.Html, false, false, 0, null, default(System.Threading.CancellationToken));
                }
            }
        }
    }
}
