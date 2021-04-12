using CliFx.Infrastructure;
using CliFx.Input;
using CliFx.Schema;
using CliFx.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CliFx.Suggestions
{
    internal class SuggestionService
    {
        private ApplicationSchema _applicationSchema;
        private readonly IFileSystem _fileSystem;
        private readonly IReadOnlyList<EnvironmentVariableInput> _environmentVariableInputs;

        public SuggestionService(ApplicationSchema applicationSchema, IFileSystem fileSystem, IReadOnlyList<EnvironmentVariableInput> environmentVariableInputs)
        {
            _applicationSchema = applicationSchema;
            _fileSystem = fileSystem;
            _environmentVariableInputs = environmentVariableInputs;
        }

        public bool ShouldInstallHooks(CommandInput commandInput)
        {
            return commandInput.Options.Any(p => p.Identifier == "install");
        }

        public IEnumerable<string> GetSuggestions(CommandInput commandInput)
        {
            var text = ExtractCommandText(commandInput);
            var suggestArgs = CommandLineSplitter.Split(text).Skip(1);    // ignore the application name

            var suggestInput = CommandInput.Parse(
                   suggestArgs.ToArray(),
                   _environmentVariableInputs.ToDictionary(p => p.Name, p => p.Value),
                   _applicationSchema.GetCommandNames());

            var commandMatch = _applicationSchema.Commands
                                   .FirstOrDefault(p => string.Equals(p.Name, suggestInput.CommandName, StringComparison.OrdinalIgnoreCase));

            // suggest a command name if we don't have an exact match
            if (commandMatch == null)
            {
                return _applicationSchema.GetCommandNames()
                             .Where(p => p.StartsWith(suggestInput.CommandName, StringComparison.OrdinalIgnoreCase))
                             .OrderBy(p => p)
                             .ToList();
            }

            // handle options for the command we found
            var option = suggestInput.Options.LastOrDefault();
            if (option != null)
            {
                if( commandMatch.Options.Any(p => p.MatchesIdentifier(option.Identifier)))
                {
                    // Don't return any suggestions for exact option matches
                    return NoSuggestions();
                }

                if (option.RawText.StartsWith("--"))
                {
                    return commandMatch.Options
                                       .Where(p => p.Name != null && p.Name.StartsWith(option.Identifier, StringComparison.OrdinalIgnoreCase))
                                       .Select(p => $"--{p.Name}");
                }
                return NoSuggestions();
            }

            // the parser returns a parameter for "--" or "-"
            var lastParameter = suggestInput.Parameters.LastOrDefault();
            if (lastParameter?.Value == "--")
            {
                return commandMatch.Options.OrderBy(p => p.Name).Select(p => $"--{p.Name}");
            }

            if (lastParameter?.Value == "-")
            {
                return commandMatch.Options.OrderBy(p => p.ShortName).Select(p => $"-{p.ShortName}")
                        .Concat(commandMatch.Options.Select(p => $"--{p.Name}"));
            }

            return NoSuggestions();
        }

        private string ExtractCommandText(CommandInput input)
        {
            // Accept command line arguments via environment variable as a workaround to powershell escape sequence shennidgans
            var commandCacheVariable = input.Options.FirstOrDefault(p => p.Identifier == "envvar")?.Values[0];

            if (commandCacheVariable == null)
            {
                // ignore cursor position as we don't know what the original user input string really is
                return string.Join(" ", input.OriginalCommandLine.Where(arg => !IsDirective(arg)));
            }

            var command = input.EnvironmentVariables.FirstOrDefault(p => string.Equals(p.Name, commandCacheVariable))?.Value ?? "";
            var cursorPositionText = input.Options.FirstOrDefault(p => p.Identifier == "cursor")?.Values[0];
            var cursorPosition = command.Length;

            if (int.TryParse(cursorPositionText, out cursorPosition) && cursorPosition < command.Length)
            {
                return command.Remove(cursorPosition);
            }
            return command;
        }

        private static bool IsDirective(string arg)
        {
            return arg.StartsWith('[') && arg.EndsWith(']');
        }

        private static List<string> NoSuggestions()
        {
            return new List<string>();
        }
    }
}

