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
                   _applicationSchema.GetCommandNames(), true);

            var commandSchema = _applicationSchema.Commands
                                   .FirstOrDefault(p => string.Equals(p.Name, suggestInput.CommandName, StringComparison.OrdinalIgnoreCase));

            // suggest a command name if we don't have an exact match
            if (commandSchema == null)
            {
                // handle completions of incomplete child command names
                // where the remaining segment of the command name must be supplied (and not the complete command name)
                var segments = _applicationSchema.GetCommandNames()
                    .Where(p => p.StartsWith(suggestInput.CommandName, StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.Split());

                var inputSegments = suggestInput.CommandName?.Split() ?? new string[] { };
                var completeSegementCount = Math.Max(0, inputSegments.Count() -1 );

                return segments.Select(p => string.Join(" ", p.Skip(completeSegementCount).ToArray()));
            }

            // prioritise option suggestions over parameter suggestions, as there might be an 
            // unlimited suggestions for parameters where a series of parameters is expected
            // edge case: CommandInput.Parse() returns a parameter for "--" or "-", so get this case out of the way first.
            var lastParameter = suggestInput.Parameters.LastOrDefault();
            if (lastParameter?.Value == "--")
            {
                return commandSchema.Options.Select(p => $"--{p.Name}");
            }

            if (lastParameter?.Value == "-")
            {
                return commandSchema.Options.Select(p => $"-{p.ShortName}").Concat(commandSchema.Options.Select(p => $"--{p.Name}"));
            }

            var optionInput = suggestInput.Options.LastOrDefault();
            if (optionInput != null)
            {
                return ProvideSuggestionsForOptionInputs(commandSchema, optionInput);
            }

            // provide parameter suggestions
            try
            {
                var parameterBindings = GetParameterBindings(suggestInput.Parameters, commandSchema.Parameters);
                if (lastParameter != null)
                {
                    var schema = parameterBindings[lastParameter];

                    var validParameterValues = schema.Property.GetValidValues()
                                   .Select(p => p == null ? "" : p.ToString())
                                   .Where(p => p.StartsWith(lastParameter.Value, StringComparison.OrdinalIgnoreCase));

                    if (validParameterValues.Any(p => string.Equals(p, lastParameter.Value, StringComparison.OrdinalIgnoreCase)))
                    {
                        return NoSuggestions();
                    }

                    return validParameterValues;
                }
            }
            catch (InvalidOperationException)
            {
                // parameters outnumber schemas, no way to make any suggestions. 
                return NoSuggestions();
            }

            return NoSuggestions();
        }

        private Dictionary<ParameterInput, ParameterSchema> GetParameterBindings(IReadOnlyList<ParameterInput> inputs, IReadOnlyList<ParameterSchema> schemas)
        {
            var queue = new Queue<ParameterSchema>(schemas.OrderBy(p => p.Order));
            var dictionary = new Dictionary<ParameterInput, ParameterSchema>();

            foreach (var input in inputs)
            {
                var schema = queue.Peek().Property.IsScalar() ? queue.Dequeue() : queue.Peek();
                dictionary.Add(input, schema);
            }

            return dictionary;
        }

        private static IEnumerable<string> ProvideSuggestionsForOptionInputs(CommandSchema commandSchema, OptionInput optionInput)
        {
            bool exactOptionMatchFound = commandSchema.Options.Any(
                                            p => string.Equals($"--{p.Name}", optionInput.RawText, StringComparison.OrdinalIgnoreCase)
                                              || string.Equals($"-{p.ShortName}", optionInput.RawText));

            if (exactOptionMatchFound)
            {
                return NoSuggestions();
            }

            if (optionInput.RawText.StartsWith("--"))
            {
                return commandSchema.Options
                                   .Where(p => p.Name != null && p.Name.StartsWith(optionInput.Identifier, StringComparison.OrdinalIgnoreCase))
                                   .Select(p => $"--{p.Name}");
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

