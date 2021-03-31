using System;
using System.IO;

namespace CliFx.Suggestions
{
    class UnixPowershellEnvironment : WindowsPowershellEnvironment
    {
        public override string[] SupportedShellPaths => new[] { @"/usr/bin/pwsh" };

        public override string InstallPath => Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify),
                        "powershell",
                        "Microsoft.PowerShell_profile.ps1");
    }
}
