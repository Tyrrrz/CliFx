namespace CliFx.Directives
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using CliFx.Attributes;
    using CliFx.Internal;

    /// <summary>
    /// When application is ran in debug mode (using the [debug] directive), it will wait for debugger to be attached before proceeding.
    /// This is useful for debugging apps that were ran outside of the IDE.
    /// </summary>
    [Directive("debug", Description = "Starts a debugging mode. Application will wait for debugger to be attached before proceeding.")]
    public sealed class DebugDirective : IDirective
    {
        /// <inheritdoc/>
        public bool ContinueExecution => true;

        /// <inheritdoc/>
        public async ValueTask HandleAsync(IConsole console)
        {
            var processId = ProcessEx.GetCurrentProcessId();

            console.WithForegroundColor(ConsoleColor.Green, () =>
                console.Output.WriteLine($"Attach debugger to PID {processId} to continue."));

            Debugger.Launch();

            while (!Debugger.IsAttached)
                await Task.Delay(100);

            console.WithForegroundColor(ConsoleColor.Green, () =>
                console.Output.WriteLine($"Debugger attached to PID {processId}."));
        }
    }
}
