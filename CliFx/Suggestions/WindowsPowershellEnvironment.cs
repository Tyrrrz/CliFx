using System;
using System.IO;

namespace CliFx.Suggestions
{
    class WindowsPowershellEnvironment : ISuggestEnvironment
    {
        public string Version => "V1";

        public virtual string[] SupportedShellPaths => new[] { @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" };

        public virtual string InstallPath => Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify),
                "WindowsPowerShell",
                "Microsoft.PowerShell_profile.ps1");

        public virtual string GetInstallCommand(string commandName)
        {
            return $@"
### clifx-suggest-begins-here-{commandName}-{Version}
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
### clifx-suggest-ends-here-{commandName}";
        }                
    }
}
