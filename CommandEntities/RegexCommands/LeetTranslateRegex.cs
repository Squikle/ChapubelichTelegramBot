using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.RegexCommands
{
    class LeetTranslateRegex : RegexCommand
    {
        public override string Pattern => @"^\/? *(лит|литспик|leet|leetspeak|1337) *(-n|-l)? ?([^ ][\s\S]*?)?$";

        private static readonly string[] LeetTable =
        {
                "4",
                "8",
                "(",
                "[)",
                "3",
                "]=",
                "6",
                "|-|",
                "!",
                "_|",
                "|<",
                "1",
                "|\\/|",
                "|\\|",
                "0",
                "|*",
                "0_",
                "|2",
                "5",
                "7",
                "(_)",
                "\\/",
                "\\^/",
                "><",
                "'/",
                "2",
        };
        private static readonly string[] CyrillicTable =
        {
                "a",
                "b",
                "v",
                "g",
                "d",
                "e",
                "zh",
                "z",
                "i",
                "y",
                "k",
                "l",
                "m",
                "n",
                "o",
                "p",
                "r",
                "s",
                "t",
                "u",
                "f",
                "h",
                "ts",
                "ch",
                "sh",
                "sh",
                "",
                "i",
                "\'",
                "e",
                "yu",
                "ya",
        };

        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            Match match = Regex.Match(message.Text, Pattern, RegexOptions.IgnoreCase);
            string textToTranslate = match.Groups[3].Value.ToLower();

            if (string.IsNullOrEmpty(textToTranslate))
            {
                await client.TrySendTextMessageAsync(message.Chat.Id,
                    "Чтобы воспользоваться leet переводчиком введи текст после команды /leet 😉",
                    replyToMessageId: message.MessageId);
                return;
            }

            string modifier = match.Groups[2].Value;
            string answer = String.Empty;

            if (!string.IsNullOrEmpty(modifier))
                switch (modifier[1])
                {
                    case 'l':
                        answer = ToLeet(textToTranslate);
                        break;
                    case 'n':
                        answer = FromLeet(textToTranslate);
                        break;
                }
            else
            {
                int letters = 0;
                int notLetters = 0;
                foreach (char sym in textToTranslate)
                {
                    if ((sym >= 97 && sym <= 122) || (sym >= 1072 && sym <= 1103))
                        letters++;
                    else notLetters++;
                }

                answer = letters >= notLetters ? ToLeet(textToTranslate) : FromLeet(textToTranslate);
            }

            if (String.IsNullOrEmpty(answer))
                return;

            if (answer.Length > 4000)
            {
                await client.TrySendTextMessageAsync(message.Chat.Id,
                "Сообщение слишком длинное😔",
                replyToMessageId: message.MessageId);
                return;
            }
            await client.TrySendTextMessageAsync(message.Chat.Id,
                answer,
                replyToMessageId: message.MessageId);
        }
        private static string FromLeet(string textToTranslate)
        {
            StringBuilder translatedString = new StringBuilder();
            for (int pivot = 0; pivot < textToTranslate.Length;)
            {
                var translatedLetter = String.Empty;
                switch (textToTranslate[pivot])
                {
                    case '4':
                        translatedLetter = "a";
                        pivot++;
                        break;

                    case '8':
                        translatedLetter = "b";
                        pivot++;
                        break;

                    case '(':
                        {
                            if (pivot + 2 < textToTranslate.Length && textToTranslate.Substring(pivot + 1, 2) == "_)")
                            {
                                translatedLetter = "u";
                                pivot += 3;
                            }
                            else
                            {
                                translatedLetter = "c";
                                pivot++;
                            }
                            break;
                        }

                    case '[':
                        {
                            if (pivot + 1 < textToTranslate.Length && textToTranslate[pivot + 1] == ')')
                            {
                                translatedLetter = "d";
                                pivot += 2;
                                break;
                            }
                            pivot++;
                            break;
                        }

                    case '3':
                        translatedLetter = "e";
                        pivot++;
                        break;

                    case ']':
                        {
                            if (pivot + 1 < textToTranslate.Length && textToTranslate[pivot + 1] == '=')
                            {
                                translatedLetter = "f";
                                pivot += 2;
                                break;
                            }
                            pivot++;
                            break;
                        }

                    case '6':
                        translatedLetter = "g";
                        pivot++;
                        break;

                    case '|':
                        {
                            if (pivot + 1 < textToTranslate.Length)
                            {
                                if (textToTranslate[pivot + 1] == '<')
                                {
                                    translatedLetter = "k";
                                    pivot += 2;
                                    break;
                                }
                                else if (textToTranslate[pivot + 1] == '*')
                                {
                                    translatedLetter = "p";
                                    pivot += 2;
                                    break;
                                }
                                else if (textToTranslate[pivot + 1] == '2')
                                {
                                    translatedLetter = "r";
                                    pivot += 2;
                                    break;
                                }
                                else if (pivot + 2 < textToTranslate.Length && textToTranslate.Substring(pivot + 1, 2) == "-|")
                                {
                                    translatedLetter = "h";
                                    pivot += 3;
                                    break;
                                }
                                else if (pivot + 2 < textToTranslate.Length && textToTranslate.Substring(pivot + 1, 2) == "\\|")
                                {
                                    translatedLetter = "n";
                                    pivot += 3;
                                    break;
                                }
                                else if (pivot + 3 < textToTranslate.Length && textToTranslate.Substring(pivot + 1, 3) == "\\/|")
                                {
                                    translatedLetter = "m";
                                    pivot += 4;
                                    break;
                                }
                            }
                            pivot++;
                            break;
                        }

                    case '!':
                        translatedLetter = "i";
                        pivot++;
                        break;

                    case '_':
                        {
                            if (pivot + 1 < textToTranslate.Length && textToTranslate[pivot + 1] == '|')
                            {
                                translatedLetter = "j";
                                pivot += 2;
                                break;
                            }
                            pivot++;
                            break;
                        }

                    case '1':
                        translatedLetter = "l";
                        pivot++;
                        break;

                    case '0':
                        if (pivot + 1 < textToTranslate.Length
                            && textToTranslate[pivot + 1] == '_')
                        {
                            translatedLetter = "q";
                            pivot += 2;
                            break;
                        }
                        else
                        {
                            translatedLetter = "o";
                            pivot++;
                            break;
                        }

                    case '5':
                        translatedLetter = "s";
                        pivot++;
                        break;

                    case '7':
                        translatedLetter = "t";
                        pivot++;
                        break;

                    case '\\':
                        if (pivot + 2 < textToTranslate.Length && textToTranslate.Substring(pivot + 1, 2) == "^/")
                        {
                            translatedLetter = "w";
                            pivot += 3;
                            break;
                        }
                        else if (pivot + 1 < textToTranslate.Length && textToTranslate[pivot + 1] == '/')
                        {
                            translatedLetter = "v";
                            pivot += 2;
                            break;
                        }
                        pivot++;
                        break;

                    case '>':
                        if (pivot + 1 < textToTranslate.Length && textToTranslate[pivot + 1] == '<')
                        {
                            translatedLetter = "x";
                            pivot += 2;
                            break;
                        }
                        pivot++;
                        break;

                    case '\'':
                        if (pivot + 1 < textToTranslate.Length && textToTranslate[pivot + 1] == '/')
                        {
                            translatedLetter = "y";
                            pivot += 2;
                            break;
                        }
                        pivot++;
                        break;

                    case '2':
                        translatedLetter = "z";
                        pivot++;
                        break;

                    case ' ':
                        if (pivot + 1 < textToTranslate.Length && textToTranslate[pivot + 1] == ' ')
                        {
                            translatedLetter = " ";
                            pivot += 2;
                            break;
                        }
                        pivot++;
                        break;

                    case '\"':
                        pivot++;
                        while (pivot < textToTranslate.Length && textToTranslate[pivot] != '\"')
                        {
                            translatedLetter += textToTranslate[pivot];
                            pivot++;
                        }
                        pivot++;
                        break;

                    default:
                        translatedLetter = textToTranslate[pivot].ToString();
                        pivot++;
                        break;
                }
                translatedString.Append(translatedLetter);
            }
            return translatedString.ToString();
        }
        private static string ToLeet(string textToTranslate)
        {
            StringBuilder convertedToCyrillic = new StringBuilder(textToTranslate.Length);

            foreach (var tSymbol in textToTranslate)
            {
                if (tSymbol >= 1072 && tSymbol <= 1103)
                {
                    convertedToCyrillic.Append(CyrillicTable[tSymbol - 1072]);
                }
                else if (tSymbol == 1105)
                    convertedToCyrillic.Append('e');
                else
                {
                    convertedToCyrillic.Append(tSymbol);
                }
            }

            textToTranslate = convertedToCyrillic.ToString().ToLower();
            StringBuilder translatedString = new StringBuilder();
            bool invalid = false;
            foreach (var tSymbol in textToTranslate)
            {
                if (tSymbol == ' ')
                {
                    if (invalid)
                        translatedString.Append("\" ");
                    translatedString.Append("  ");
                }
                else if (tSymbol >= 97 && tSymbol <= 122)
                {
                    if (invalid)
                        translatedString.Append("\" ");
                    translatedString.Append(LeetTable[tSymbol - 97]);
                }
                else
                {
                    if (!invalid)
                    {
                        invalid = true;
                        translatedString.Append(" \"");
                    }
                    translatedString.Append(tSymbol);

                    continue;
                }

                invalid = false;
            }
            if (invalid)
                translatedString.Append("\" ");

            return translatedString.ToString();
        }
    }
}
