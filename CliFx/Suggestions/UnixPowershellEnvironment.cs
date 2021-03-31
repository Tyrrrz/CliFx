using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CliFx.Suggestions
{
    class UnixPowershellEnvironment : WindowsPowershellEnvironment
    {
        public override string[] ShellPaths => new[] { @"/usr/bin/pwsh" };

        public override string GetInstallPath()
        {
            return Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        ".config",
                        "powershell",
                        "Microsoft.PowerShell_profile.ps1");
        }
    }
}
