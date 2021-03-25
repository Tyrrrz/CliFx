using System;
using System.Collections.Generic;
using System.Text;

namespace CliFx.Utils
{
    internal static class CommandLineSplitter
    {
        /// <summary>
        ///  Reproduces Environment.GetCommandLineArgs() as per https://docs.microsoft.com/en-us/dotnet/api/system.environment.getcommandlineargs?view=net-5.0
        ///
        ///  Input at the command line Resulting command line arguments
        ///  MyApp alpha beta                                MyApp, alpha, beta
        ///  MyApp "alpha with spaces" "beta with spaces"    MyApp, alpha with spaces, beta with spaces
        ///  MyApp 'alpha with spaces' beta                  MyApp, 'alpha, with, spaces', beta
        ///  MyApp \\\alpha \\\\"beta	                     MyApp, \\\alpha, \\beta
        ///  MyApp \\\\\"alpha \"beta	                     MyApp, \\"alpha, "beta
        ///
        ///  Used to parse autocomplete text as it is passed in as a single argument by Powershell
        ///        
        /// </summary>
        public static string[] Split(string s)
        {
            int escapeSequenceLength = 0;
            int escapeSequenceEnd = 0;
            bool ignoreSpaces = false;

            var tokens = new List<string>();
            StringBuilder tokenBuilder = new StringBuilder();

            for (int i = 0; i < s.Length; i++)
            {
                // determine how long the escape character sequence is 
                if (s[i] == '\\' && i > escapeSequenceEnd)
                {
                    for (int j = i; j < s.Length; j++)
                    {
                        if (s[j] == '\\')
                        {
                            continue;
                        }
                        else if (s[j] != '\"')
                        {
                            // edge case: \\\alpha --> \\\alpha (no escape)
                            escapeSequenceLength = 0;
                            break;
                        }

                        escapeSequenceLength = j - i;

                        // edge case: \\\\"beta -> \\beta
                        // treat the " as an escape character so that we skip over it
                        if (escapeSequenceLength == 4)
                        {
                            escapeSequenceLength = 6;
                        }
                        else
                        {
                            // capture the escaped character in our escape sequence
                            escapeSequenceLength++;
                        }

                        escapeSequenceEnd = i + escapeSequenceLength;
                        break;
                    }
                }

                if (escapeSequenceLength > 0 && escapeSequenceLength % 2 == 0)
                {
                    // skip escape characters
                }
                else
                {
                    bool characterIsEscaped = escapeSequenceLength != 0;

                    // edge case: '"' character is used to divide tokens eg: MyApp "alpha with spaces" "beta with spaces" 
                    // skip the '"' character
                    if (!characterIsEscaped && s[i] == '"')
                    {
                        ignoreSpaces = !ignoreSpaces;
                    }
                    // edge case: ' ' character is used to divide tokens
                    else if (!characterIsEscaped && char.IsWhiteSpace(s[i]) && !ignoreSpaces)
                    {
                        tokens.Add(tokenBuilder.ToString());
                        tokenBuilder.Clear();
                    }
                    else
                    {
                        tokenBuilder.Append(s[i]);
                    }
                }

                if (escapeSequenceLength > 0)
                {
                    escapeSequenceLength--;
                }
            }

            var token = tokenBuilder.ToString();
            if (!string.IsNullOrWhiteSpace(token))
            {
                tokens.Add(token);
            }
            return tokens.ToArray();
        }
    }
}
