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
using System.IO;

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

            BotPrivateCommands = new List<Command>()
            {
                new HelloCommand(),

                new SetShutdownCommand(),
                new CancelShutdownCommand(),

                new BalanceCommand(),
                new MenuCommand(),
                new HelpCommand(),
                new SettingsCommand(),
                new GenderChangeCommand(),
                new DefaultBetChangeCommand(),
                new MyProfileCommand(),
                new DailyRewardCommand(),

                new GamesCommand(),
                new RouletteStartCommand(),
            };
            BotGroupCommands = new List<Command>()
            {

            };
            BotCallbackMessages = new List<CallBackMessage>()
            {
                new DefaultBetChangeCallbackMessage(),
                new RouletteStartCallbackMessage(),
                new RouletteBetColorCallbackMessage(),
                new RouletteBetNumbersCallbackMessage(),
                new RouletteRollCallbackMessage(),
                new RouletteBetCancelCallbackMessage(),
            };
            BotRegexCommands = new List<RegexCommand>()
            {
                new RouletteColorBetRegexCommand(),
                new RouletteStartRegexCommand(),
                new RouletteRollRegexCommand(),
                new RouletteBetCancelRegexCommand(),
                new RouletteCheckBetRegexCommand(),
                new RouletteNumberBetRegexCommand(),

                new BalanceRegexCommand(),
                new TransferRegexCommand(),

                new IsItGameRegexCommand(),

                new LeetTranslateRegexCommand(),
            };
            BotAdminRegexCommands = new List<RegexCommand>()
            {
                new SendAllRegexCommand(),
                new EchoRegexCommand(),
            };

#if (DEBUG)
            string ApiKey = GetKeys().GetValue<string>("ApiKeys:DebugKey");
#else
            string ApiKey = GetKeys().GetValue<string>("ApiKeys:ReleaseKey");
#endif
            _client = new TelegramBotClient(ApiKey) { Timeout = TimeSpan.FromSeconds(10) };
            return _client;
        }
        public static IConfiguration GetConfig()
            => new ConfigurationBuilder().AddJsonFile($"./Init/Config/BotConfig.json").Build();
        public static IConfiguration GetKeys()
            => new ConfigurationBuilder().AddJsonFile($"./Init/Config/Keys.json").Build();
            

        public static StartCommand                      StartCommand;
        public static RegistrationCommand               RegistrationCommand;
        public static GenderCallbackMessage             GenderCallbackMessage;

        private static List<Command>                    BotPrivateCommands;
        private static List<Command>                    BotGroupCommands;
        private static List<RegexCommand>               BotRegexCommands;
        private static List<RegexCommand>               BotAdminRegexCommands;
        private static List<CallBackMessage>            BotCallbackMessages;
        public static IReadOnlyList<Command>            BotPrivateCommandsList { get => BotPrivateCommands.AsReadOnly(); }
        public static IReadOnlyList<Command>            BotGroupCommandsList { get => BotGroupCommands.AsReadOnly(); }
        public static IReadOnlyList<RegexCommand>       BotRegexCommandsList { get => BotRegexCommands.AsReadOnly(); }
        public static IReadOnlyList<RegexCommand>       BotAdminRegexCommandsList { get => BotAdminRegexCommands.AsReadOnly(); }
        public static IReadOnlyList<CallBackMessage>    CallBackMessagesList { get => BotCallbackMessages.AsReadOnly(); }
    }
}
