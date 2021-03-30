using CliFx.Infrastructure;
using CliFx.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace CliFx.Suggestions
{
    interface ISuggestEnvironment
    {
        bool ShouldInstall();

        string Version { get; }

        string GetInstallPath();

        string GetInstallCommand(string command);
    }
}
