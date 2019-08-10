using System;
using System.Reflection;
using CliFx.Services;

namespace CliFx
{
    public interface ICliApplicationBuilder
    {
        ICliApplicationBuilder WithCommand(Type commandType);

        ICliApplicationBuilder WithCommandsFrom(Assembly commandAssembly);

        ICliApplicationBuilder UseTitle(string title);

        ICliApplicationBuilder UseExecutableName(string exeName);

        ICliApplicationBuilder UseVersionText(string version);

        ICliApplicationBuilder UseConsole(IConsole console);

        ICliApplicationBuilder UseCommandFactory(ICommandFactory factory);

        ICliApplication Build();
    }
}