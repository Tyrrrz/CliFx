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

        public SuggestionService(ApplicationSchema applicationSchema, IFileSystem fileSystem)
        {
            _applicationSchema = applicationSchema;
            _fileSystem = fileSystem;
        }

        public IEnumerable<string> GetSuggestions(CommandInput commandInput)
        {
            var text = ExtractCommandText(commandInput);
            var suggestArgs = CommandLineSplitter.Split(text).Skip(1);    // ignore the application name

            var suggestInput = CommandInput.Parse(
                   suggestArgs.ToArray(),
                   commandInput.EnvironmentVariables.ToDictionary(p => p.Name, p => p.Value),
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

        public void EnsureInstalled(string commandName)
        {
            foreach (var env in new ISuggestEnvironment[] { new BashSuggestEnvironment(), new PowershellSuggestEnvironment() })
            {
                var path = env.GetInstallPath();

                if(!env.ShouldInstall())
                {
                    continue;
                }

                if (!_fileSystem.Exists(path))
                {
                    _fileSystem.WriteAllText(path, "");
                }

                var pattern = $"### clifx-suggest-begins-here-{Regex.Escape(commandName)}-{env.Version}.*### clifx-suggest-ends-here-{Regex.Escape(commandName)}";
                var script = _fileSystem.ReadAllText(path);
                var match = Regex.Match(script, pattern, RegexOptions.Singleline);
                if (match.Success)
                {
                    continue;
                }

                var uninstallPattern = $"### clifx-suggest-begins-here-{Regex.Escape(commandName)}.*### clifx-suggest-ends-here-{Regex.Escape(commandName)}";
                var sb = new StringBuilder(Regex.Replace(script, uninstallPattern, "", RegexOptions.Singleline));
                sb.AppendLine(env.GetInstallCommand(commandName));

                // move backup to temp folder for OS to delete eventually (just in case something really bad happens)
                var tempFile = Path.GetFileName(path);
                var tempExtension = Path.GetExtension(tempFile) + $".backup_{DateTime.UtcNow.ToFileTime()}";
                tempFile = Path.ChangeExtension(tempFile, tempExtension);
                var backupPath = Path.Combine(Path.GetTempPath(), tempFile);

                _fileSystem.Copy(path, backupPath);
                _fileSystem.WriteAllText(path, sb.ToString());
            }
        }
    }
}

