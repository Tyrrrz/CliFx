namespace CliFx.Suggestions
{
    interface ISuggestEnvironment
    {
        string Version { get; }

        string[] SupportedShellPaths { get; }

        string InstallPath { get; }

        string GetInstallCommand(string command);
    }
}
