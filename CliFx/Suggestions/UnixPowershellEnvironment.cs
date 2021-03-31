using System;
using System.IO;
using System.Text;

namespace CliFx.Suggestions
{
    class UnixPowershellEnvironment : WindowsPowershellEnvironment
    {
        public override string[] SupportedShellPaths => new[] { @"/usr/bin/pwsh" };

        public override string InstallPath => Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify),
                        "powershell",
                        "Microsoft.PowerShell_profile.ps1");

        public override string GetInstallCommand(string commandName)
        {
            return base.GetInstallCommand(commandName).Replace("\r", "");
        }
    }
}
