using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CliFx.Suggestions
{
    class PowershellSuggestEnvironment : ISuggestEnvironment
    {
        public string Version => "V1";
        
        public bool ShouldInstall()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                return File.Exists("/usr/bin/pwsh");

            }
            return true;
        }

        public string GetInstallPath()
        {
            var baseDir = "";
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", ".powershell");
            }
            else
            {
                var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify);
                baseDir = Path.Combine(myDocuments, "WindowsPowerShell");
            }

            return Path.Combine(baseDir, "Microsoft.PowerShell_profile.ps1");
        }

        public string GetInstallCommand(string commandName)
        {
            var safeName = commandName.Replace(" ", "_");
            return $@"
### clifx-suggest-begins-here-{safeName}-{Version}
# this block provides auto-complete for the {commandName} command
# and assumes that {commandName} is on the path
$scriptblock = {{
    param($wordToComplete, $commandAst, $cursorPosition)        
        $command = ""{commandName}""

        $commandCacheId = ""clifx-suggest-"" + (new-guid).ToString()
        Set-Content -path ""ENV:\$commandCacheId"" -value $commandAst

        $result = &$command `[suggest`] --envvar $commandCacheId --cursor $cursorPosition | ForEach-Object {{
                [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
         }}

        Remove-Item -Path ""ENV:\$commandCacheId""
        $result
}}

Register-ArgumentCompleter -Native -CommandName ""{commandName}"" -ScriptBlock $scriptblock
### clifx-suggest-ends-here-{safeName}";
        }
    }
}
