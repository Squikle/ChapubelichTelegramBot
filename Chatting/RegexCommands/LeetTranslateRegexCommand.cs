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

namespace ChapubelichBot.Chatting.RegexCommands
{
    class LeetTranslateRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/ *((в )?лит(спик)?|(to )?leet(speak)?)( -(n|l))? +(([^\r\n\t\f\v= ]+ *)+)$";

        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            Match match = Regex.Match(message.Text, Pattern);
            string textToTranslate = match.Groups[8].Value.ToLower();
            string modifier = match.Groups[7].Value;

            string answer = String.Empty;

            if (!string.IsNullOrEmpty(modifier))
                switch (modifier)
                {
                    case "l":
                        answer = ToLeet(textToTranslate);
                        break;
                    case "n":
                        answer = FromLeet(textToTranslate);
                        break;
                }
            else
            {
                int letters = 0;
                int notLetters = 0;
                foreach (char sym in textToTranslate)
                {
                    if (sym >= 97 && sym <= 122)
                        letters++;
                    else notLetters++;
                }

                if (letters >= notLetters)
                    answer = ToLeet(textToTranslate);
                else 
                    answer = FromLeet(textToTranslate);
            }

            if (String.IsNullOrEmpty(answer))
                return;

            await client.TrySendTextMessageAsync(message.Chat.Id,
                answer, 
                replyToMessageId: message.MessageId);
        }
        static string FromLeet(string textToTranslate)
        {
            StringBuilder translatedString = new StringBuilder();
            string translatedLetter;
            for (int pivot = 0; pivot < textToTranslate.Length;)
            {
                translatedLetter = String.Empty;
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
                                break;
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
                                    pivot += 3;
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

                    case '#':
                        while (pivot < textToTranslate.Length &&
                            textToTranslate[pivot + 1] >= 48 &&
                            textToTranslate[pivot + 1] <= 57)
                        {
                            pivot++;
                            translatedLetter += textToTranslate[pivot];
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
        static string ToLeet(string textToTranslate)
        {
            string[] leetTable = new string[]
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

            textToTranslate = textToTranslate.ToLower();
            StringBuilder translatedString = new StringBuilder();
            bool isNumber = false;
            bool invalid = false;
            for (int currentSymbol = 0; currentSymbol < textToTranslate.Length; currentSymbol++)
            {
                if (textToTranslate[currentSymbol] >= 97 && textToTranslate[currentSymbol] <= 122)
                {
                    if (invalid)
                        translatedString.Append($"\" ");
                    translatedString.Append(leetTable[textToTranslate[currentSymbol] - 97]);
                }
                else if (textToTranslate[currentSymbol] == ' ')
                {
                    if (invalid)
                        translatedString.Append($"\" ");
                    translatedString.Append("  ");
                }
                else if (textToTranslate[currentSymbol] >= 48 && textToTranslate[currentSymbol] <= 57)
                {
                    if (invalid)
                        translatedString.Append($"\" ");
                    if (!isNumber)
                    {
                        isNumber = true;
                        translatedString.Append("#");
                    }
                    translatedString.Append(textToTranslate[currentSymbol]);

                    continue;
                }
                else
                {
                    if (!invalid)
                    {
                        invalid = true;
                        translatedString.Append($" \"");
                    }
                    translatedString.Append(textToTranslate[currentSymbol]);

                    continue;
                }

                isNumber = false;
                invalid = false;
            }
            if (invalid)
                translatedString.Append("\" ");

            return translatedString.ToString();
        }
    }
}
