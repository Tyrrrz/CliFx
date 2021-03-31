using CliFx.Infrastructure;
using CliFx.Input;
using System;
using System.Collections.Generic;
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
            foreach (var env in new ISuggestEnvironment[] { new BashEnvironment(), new WindowsPowershellEnvironment(), new UnixPowershellEnvironment() })
            {
                if( !env.ShellPaths.Any(p=> _fileSystem.Exists(p)))
                {
                    continue;
                }

                var path = env.GetInstallPath();
                
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

                // backup to temp folder for OS to delete eventually (just in case something really bad happens)
                var tempFile = Path.GetFileName(path);
                var tempExtension = Path.GetExtension(tempFile) + $".backup_{DateTime.UtcNow.ToFileTime()}";
                tempFile = Path.ChangeExtension(tempFile, tempExtension);
                var backupPath = Path.Combine(Path.GetTempPath(), tempFile);

                _fileSystem.Copy(path, backupPath);
                _fileSystem.WriteText(path, sb.ToString());
            }
        }
    }
}
