using System;
using System.Reflection;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Models;
using CliFx.Services;

namespace CliFx
{
    public abstract class Command : ICommand
    {
        [CommandOption("help", 'h', GroupName = "__help", Description = "Shows help.")]
        public bool IsHelpRequested { get; set; }

        [CommandOption("version", GroupName = "__version", Description = "Shows application version.")]
        public bool IsVersionRequested { get; set; }

        public CommandContext Context { get; set; }

        public IConsoleWriter Output { get; set; } = ConsoleWriter.GetStandardOutput();

        public IConsoleWriter Error { get; set; } = ConsoleWriter.GetStandardError();

        protected virtual ExitCode Process() => throw new InvalidOperationException(
            "Can't execute command because its execution method is not defined. " +
            $"Override {nameof(Process)} or {nameof(ProcessAsync)} on {GetType().Name} in order to make it executable.");

        protected virtual Task<ExitCode> ProcessAsync() => Task.FromResult(Process());

        protected virtual void ShowHelp()
        {
            var text = new HelpTextBuilder().Build(Context);
            Output.WriteLine(text);
        }

        protected virtual void ShowVersion()
        {
            var text = Assembly.GetEntryAssembly()?.GetName().Version.ToString();
            Output.WriteLine(text);
        }

        public Task<ExitCode> ExecuteAsync()
        {
            if (IsHelpRequested)
            {
                ShowHelp();
                return Task.FromResult(ExitCode.Success);
            }

            if (IsVersionRequested && Context.CommandSchema.IsDefault)
            {
                ShowVersion();
                return Task.FromResult(ExitCode.Success);
            }

            return ProcessAsync();
        }
    }
}