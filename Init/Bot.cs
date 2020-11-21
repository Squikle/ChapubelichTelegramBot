using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Chatting.Commands;
using ChapubelichBot.Chatting.CallbackMessages;
using ChapubelichBot.Chatting.RegexCommands;
using System;
using System.Collections.Generic;
using Telegram.Bot;
using ChapubelichBot.Chatting.Commands.AdminCommands;
using ChapubelichBot.Chatting.RegexCommands.AdminRegexCommands;
using Microsoft.Extensions.Configuration;

namespace ChapubelichBot.Init
{
    public static class Bot
    {
        private static ITelegramBotClient _client;
        public static ITelegramBotClient GetClient()
        {
            if (_client != null)
                return _client;

            StartCommand = new StartCommand();
            RegistrationCommand = new RegistrationCommand();
            GenderCallbackMessage = new GenderCallbackMessage();

            _botPrivateCommands = new List<Command>()
            {
                new HelloCommand(),

                new BalanceCommand(),
                new MenuCommand(),
                new HelpCommand(),
                new SettingsCommand(),
                new GenderChangeCommand(),
                new DefaultBetChangeCommand(),
                new ProfileInfoCommand(),
                new DailyRewardCommand(),

                new GamesCommand(),
                new RouletteStartCommand(),
            };
            _botGroupRegexCommands = new List<RegexCommand>()
            {
                new TopChatPlayersRegexCommand(),
                new PersonRollRegexCommand(),
            };
            _botCallbackMessages = new List<CallBackMessage>()
            {
                new DefaultBetChangeCallbackMessage(),
                new RouletteStartCallbackMessage(),
                new RouletteBetColorCallbackMessage(),
                new RouletteBetNumbersCallbackMessage(),
                new RouletteRollCallbackMessage(),
                new RouletteBetCancelCallbackMessage(),
            };
            _botRegexCommands = new List<RegexCommand>()
            {
                new RouletteColorBetRegexCommand(),
                new RouletteStartRegexCommand(),
                new RouletteRollRegexCommand(),
                new RouletteBetCancelRegexCommand(),
                new RouletteBetInfoRegexCommand(),
                new RouletteNumberBetRegexCommand(),
                new RouletteLogRegexCommand(),

                new BalanceRegexCommand(),
                new TransferRegexCommand(),
                new TheftRegexCommand(),

                new IsItGameRegexCommand(),

                new LeetTranslateRegexCommand(),
            };
            _botAdminRegexCommands = new List<RegexCommand>()
            {
                new SendAllRegexCommand(),
                new EchoRegexCommand(),
            };
            _botAdminCommands = new List<Command>()
            {
                new SetShutdownCommand(),
                new CancelShutdownCommand(),
                new GetIdCommand()
            };

#if (DEBUG)
            string apiKey = GetKeys().GetValue<string>("ApiKeys:DebugKey");
#else
            string apiKey = GetKeys().GetValue<string>("ApiKeys:ReleaseKey");
#endif
            _client = new TelegramBotClient(apiKey) { Timeout = TimeSpan.FromSeconds(10) };
            return _client;
        }
        public static IConfiguration GetConfig()
            => new ConfigurationBuilder().AddJsonFile("./Init/Config/BotConfig.json").Build();
        public static IConfiguration GetKeys()
            => new ConfigurationBuilder().AddJsonFile("./Init/Config/Keys.json").Build();
            

        public static StartCommand                      StartCommand;
        public static RegistrationCommand               RegistrationCommand;
        public static GenderCallbackMessage             GenderCallbackMessage;

        private static List<Command>                    _botPrivateCommands;
        private static List<Command>                    _botAdminCommands;
        private static List<RegexCommand>               _botGroupRegexCommands;
        private static List<RegexCommand>               _botRegexCommands;
        private static List<RegexCommand>               _botAdminRegexCommands;
        private static List<CallBackMessage>            _botCallbackMessages;
        public static IReadOnlyList<Command>            BotPrivateCommandsList => _botPrivateCommands.AsReadOnly();
        public static IReadOnlyList<Command>            BotAdminCommandsList => _botAdminCommands.AsReadOnly();
        public static IReadOnlyList<RegexCommand>       BotGroupRegexCommandsList => _botGroupRegexCommands.AsReadOnly();
        public static IReadOnlyList<RegexCommand>       BotRegexCommandsList => _botRegexCommands.AsReadOnly();
        public static IReadOnlyList<RegexCommand>       BotAdminRegexCommandsList => _botAdminRegexCommands.AsReadOnly();
        public static IReadOnlyList<CallBackMessage>    CallBackMessagesList => _botCallbackMessages.AsReadOnly();
    }
}
