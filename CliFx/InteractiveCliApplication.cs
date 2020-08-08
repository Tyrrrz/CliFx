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
        /// <summary>
        /// Initializes an instance of <see cref="InteractiveCliApplication"/>.
        /// </summary>
        public InteractiveCliApplication(ICliContext cliContext, ITypeActivator typeActivator) :
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Runs the application with specified command line arguments and returns the exit code.
        /// Environment variables are retrieved automatically.
        /// </summary>
        /// <remarks>
        /// If a <see cref="CommandException"/> is thrown during command execution, it will be handled and routed to the console.
        /// Additionally, if the debugger is not attached (i.e. the app is running in production), all other exceptions thrown within
        /// this method will be handled and routed to the console as well.
        /// </remarks>
        public override async ValueTask<int> RunAsync(IReadOnlyList<string> commandLineArguments)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Runs the application and returns the exit code.
        /// Command line arguments and environment variables are retrieved automatically.
        /// </summary>
        /// <remarks>
        /// If a <see cref="CommandException"/> is thrown during command execution, it will be handled and routed to the console.
        /// Additionally, if the debugger is not attached (i.e. the app is running in production), all other exceptions thrown within
        /// this method will be handled and routed to the console as well.
        /// </remarks>
        public override async ValueTask<int> RunAsync()
        {
            throw new NotImplementedException();
        }
    }
}
