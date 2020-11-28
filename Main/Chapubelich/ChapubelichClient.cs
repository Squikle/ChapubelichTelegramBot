using System;
using System.Collections.Generic;
using ChapubelichBot.CommandEntities.CallbackCommands;
using ChapubelichBot.CommandEntities.CallbackCommands.Roulette;
using ChapubelichBot.CommandEntities.Commands;
using ChapubelichBot.CommandEntities.Commands.Admin;
using ChapubelichBot.CommandEntities.RegexCommands;
using ChapubelichBot.CommandEntities.RegexCommands.AdminRegexCommands;
using ChapubelichBot.CommandEntities.RegexCommands.Roulette;
using ChapubelichBot.Main.CommandProcessors;
using ChapubelichBot.Types.Abstractions.CommandProcessors;
using ChapubelichBot.Types.Abstractions.Commands;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace ChapubelichBot.Main.Chapubelich
{
    public static class ChapubelichClient
    {
        private static ITelegramBotClient _client;
        public static ITelegramBotClient GetClient()
        {
            if (_client != null)
                return _client;

            // Обработчики ----------------------------------------------------
            AdminMessageProcessor = new AdminMessageProcessor();

            _botMessageProccessors = new List<TextMessageProcessor>
            {
                new PrivateMessageProcessor(),
                new GroupMessageProcessor(),
                new UserLeftMessageProcessor()
            };
            _botCallbackMessageProcessors = new List<CallbackMessageProcessor>
            {
                new CommonCallbackProcessor(),
            };

            // Команды ----------------------------------------------------
            StartCommand = new Start();
            RegistrationCommand = new Registration();
            GenderCallbackMessage = new GenderCallback();

            _botPrivateCommands = new List<Command>
            {
                new Hello(),

                new Balance(),
                new MenuList(),
                new Help(),
                new SettingsList(),
                new GenderSet(),
                new ComplimentSubscription(),
                new DefaultBetSet(),
                new ProfileInfo(),
                new EarnDailyReward(),

                new GamesList(),
                new RouletteStart(),
            };
            _botGroupRegexCommands = new List<RegexCommand>
            {
                new TopChatBalanceRegex(),
                new PersonRollRegex(),
            };
            _botCallbackMessages = new List<CallbackCommand>
            {
                new DefaultBetChangeCallbackMessage(),
                new RouletteStartCallbackCommand(),
                new BetColorCallback(),
                new BetNumbersCallback(),
                new RouletteRollCallback(),
                new BetCancelCallback(),
                new ComplimentSubscriptionCallback()
            };
            _botRegexCommands = new List<RegexCommand>
            {
                new BetColorRegex(),
                new RouletteStartRegex(),
                new RouletteRollRegex(),
                new BetCancelRegex(),
                new BetInfoRegex(),
                new BetNumberRegex(),
                new RouletteLogRegex(),

                new BalanceRegex(),
                new TransferRegex(),
                new TheftRegex(),

                new QuestionGameRegex(),

                new LeetTranslateRegex(),
            };
            _botAdminRegexCommands = new List<RegexCommand>
            {
                new SendAllRegex(),
                new EchoRegex(),
                new TestFeatureRegex()
            };
            _botAdminCommands = new List<Command>
            {
                new SetShutdown(),
                new CancelShutdown(),
                new GetId()
            };

#if (DEBUG)
            string apiKey = GetKeys().GetValue<string>("ApiKeys:DebugKey");
#else
            string apiKey = GetKeys().GetValue<string>("ApiKeys:ReleaseKey");
#endif
            _client = new TelegramBotClient(apiKey) { Timeout = TimeSpan.FromSeconds(300) };
            return _client;
        }
        public static IConfiguration GetConfig()
            => new ConfigurationBuilder().AddJsonFile("./Main/Config/BotConfig.json").Build();
        public static IConfiguration GetKeys()
            => new ConfigurationBuilder().AddJsonFile("./Main/Config/Keys.json").Build();
            
        public static TextMessageProcessor                          AdminMessageProcessor { get; private set; }
        public static IReadOnlyList<TextMessageProcessor>           BotMessageProcessorsList => _botMessageProccessors.AsReadOnly();
        private static List<TextMessageProcessor>                   _botMessageProccessors;
        public static IReadOnlyList<CallbackMessageProcessor>   BotCallbackMessageProcessorsList =>_botCallbackMessageProcessors.AsReadOnly();
        private static List<CallbackMessageProcessor>           _botCallbackMessageProcessors;

        public static Command                                   StartCommand { get; private set; }
        public static Command                                   RegistrationCommand { get; private set; }
        public static CallbackCommand                           GenderCallbackMessage { get; private set; }
        public static IReadOnlyList<Command>                    BotPrivateCommandsList => _botPrivateCommands.AsReadOnly();
        private static List<Command>                            _botPrivateCommands;
        public static IReadOnlyList<Command>                    BotAdminCommandsList => _botAdminCommands.AsReadOnly();
        private static List<Command>                            _botAdminCommands;
        public static IReadOnlyList<RegexCommand>               BotGroupRegexCommandsList => _botGroupRegexCommands.AsReadOnly();
        private static List<RegexCommand>                       _botGroupRegexCommands;
        public static IReadOnlyList<RegexCommand>               BotRegexCommandsList => _botRegexCommands.AsReadOnly();
        private static List<RegexCommand>                       _botRegexCommands;
        public static IReadOnlyList<RegexCommand>               BotAdminRegexCommandsList => _botAdminRegexCommands.AsReadOnly();
        private static List<RegexCommand>                       _botAdminRegexCommands;
        public static IReadOnlyList<CallbackCommand>            CallBackMessagesList => _botCallbackMessages.AsReadOnly();
        private static List<CallbackCommand>                    _botCallbackMessages;
    }
}
