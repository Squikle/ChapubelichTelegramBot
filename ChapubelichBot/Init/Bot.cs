using Chapubelich.Abstractions;
using Chapubelich.Chating.Commands;
using Chapubelich.Chatting.CallbackMessages;
using Chapubelich.Chatting.Commands;
using Chapubelich.Chatting.Commands.ShutdownCommands;
using Chapubelich.Chatting.Games.FiftyFiftyGame;
using Chapubelich.Chatting.RegexCommands;
using System;
using System.Collections.Generic;
using Telegram.Bot;

namespace Chapubelich.ChapubelichBot.Init
{
    public static class Bot
    {
        private static ITelegramBotClient _client;
        public static ITelegramBotClient Client
        {
            get
            {
                if (_client != null)
                    return _client;

                BotPrivateCommands = new List<Command>()
                {
                new HelloCommand(),
                new SetShutdownCommand(),
                new CancelShutdownCommand(),
                new BalanceCommand(),
                new MenuCommand(),
                new GamesCommand(),
                new RegistrationCommand(),
                new StartCommand(),
                new FiftyFiftyCommand(),
                };
                BotGroupCommands = new List<Command>()
                {

                };
                BotGames = new List<Game>()
                {
                    new FiftyFiftyGame()
                };
                BotCallbackMessages = new List<CallBackMessage>()
                {
                    new GenderCallbackMessage(),
                };
                BotRegexCommands = new List<RegexCommand>()
                {
                    new FiftyFiftyBetRegexCommand(),
                    new FiftyFiftyStartRegexCommand(),
                    new FiftyFiftyRollRegexCommand(),
                    new FiftyFiftyCancelRegexCommand(),
                    new FiftyFiftyCheckBetCommand(),
                    new BalanceRegexCommand()
                };

                _client = new TelegramBotClient(AppSettings.Key) { Timeout = TimeSpan.FromSeconds(10) };
                return _client;
            }
        }

        private static List<Command>            BotPrivateCommands;
        private static List<Command>            BotGroupCommands;
        private static List<RegexCommand>       BotRegexCommands;
        private static List<CallBackMessage>    BotCallbackMessages;
        private static List<Game>               BotGames;
        public static IReadOnlyList<Command>            BotPrivateCommandsList { get => BotPrivateCommands.AsReadOnly(); }
        public static IReadOnlyList<Command>            BotGroupCommandsList { get => BotGroupCommands.AsReadOnly(); }
        public static IReadOnlyList<RegexCommand>       BotRegexCommandsList { get => BotRegexCommands.AsReadOnly(); }
        public static IReadOnlyList<CallBackMessage>    CallBackMessagesList { get => BotCallbackMessages.AsReadOnly(); }
        public static IReadOnlyList<Game>               BotGamesList { get => BotGames.AsReadOnly(); }
    }
}
