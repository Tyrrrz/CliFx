using CliFx.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CliFx.Suggestions
{
    class SuggestShellHookInstaller
    {
        private readonly IFileSystem _fileSystem;

        public SuggestShellHookInstaller(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void Install(string commandName)
        {
            var detectedEnvironments = new ISuggestEnvironment[] {
                                    new BashEnvironment(),
                                    new WindowsPowershellEnvironment(),
                                    new UnixPowershellEnvironment() }
                                 .Where(env => env.SupportedShellPaths.Any(p => _fileSystem.Exists(p) ));

            foreach (var env in detectedEnvironments)
            {
                var path = env.InstallPath;
                var pattern = $"### clifx-suggest-begins-here-{Regex.Escape(commandName)}-{env.Version}.*### clifx-suggest-ends-here-{Regex.Escape(commandName)}";

                string script = "";
                _fileSystem.TryReadText(path, out script);
                var match = Regex.Match(script, pattern, RegexOptions.Singleline);
                if (match.Success)
                {
                    continue;
                }

                var uninstallPattern = $"### clifx-suggest-begins-here-{Regex.Escape(commandName)}.*### clifx-suggest-ends-here-{Regex.Escape(commandName)}";
                var sb = new StringBuilder(Regex.Replace(script, uninstallPattern, "", RegexOptions.Singleline));
                sb.AppendLine(env.GetInstallCommand(commandName));

                // create backup in case something bad happens
                var backupExtension = Path.GetExtension(path) + $".backup_{DateTime.UtcNow.ToFileTime()}";
                var backupPath = Path.ChangeExtension(path, backupExtension);

                if (_fileSystem.Exists(path))
                {
                    _fileSystem.Copy(path, backupPath);
                }
                _fileSystem.WriteText(path, sb.ToString());
            }
        }
    }
}
