using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Exceptions;

namespace CliFx
{
    /// <summary>
    /// Command line application facade.
    /// </summary>
    public partial class InteractiveCliApplication : CliApplication
    {
        private readonly ConsoleColor promptForeground = ConsoleColor.Magenta;
        private readonly ConsoleColor commandForeground = ConsoleColor.Yellow;
        private readonly ConsoleColor finishedResultForeground = ConsoleColor.White;

        /// <summary>
        /// Initializes an instance of <see cref="InteractiveCliApplication"/>.
        /// </summary>
        public InteractiveCliApplication(CliContext cliContext, ITypeActivator typeActivator) :
            base(cliContext, typeActivator)
        {

        }

        /// <summary>
        /// Runs the application with specified command line arguments and environment variables, and returns the exit code.
        /// </summary>
        /// <remarks>
        /// If a <see cref="CommandException"/> is thrown during command execution, it will be handled and routed to the console.
        /// Additionally, if the debugger is not attached (i.e. the app is running in production), all other exceptions thrown within
        /// this method will be handled and routed to the console as well.
        /// </remarks>
        public override async ValueTask<int> RunAsync(
            IReadOnlyList<string> commandLineArguments,
            IReadOnlyDictionary<string, string> environmentVariables)
        {
            return await base.RunAsync(commandLineArguments, environmentVariables);
        }
    }
}
