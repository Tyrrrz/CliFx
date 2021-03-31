using CliFx.Infrastructure;
using CliFx.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace CliFx.Suggestions
{
    interface ISuggestEnvironment
    {
        string Version { get; }

        string[] ShellPaths { get; }

        string GetInstallPath();

        string GetInstallCommand(string command);
    }
}
