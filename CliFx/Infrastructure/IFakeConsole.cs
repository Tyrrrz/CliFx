using System;

namespace CliFx.Infrastructure
{
    /// <summary>
    /// Encapsulates additional members exposed by fake implementations of <see cref="IConsole"/>.
    /// </summary>
    public interface IFakeConsole : IConsole
    {
        /// <summary>
        /// Sends a cancellation signal to the currently executing command.
        /// </summary>
        /// <remarks>
        /// If the command is not cancellation-aware (i.e. it doesn't call <see cref="IConsole.RegisterCancellation"/>),
        /// this method will not have any effect.
        /// </remarks>
        void RequestCancellation(TimeSpan delay);

        /// <inheritdoc cref="RequestCancellation(System.TimeSpan)" />
        void RequestCancellation();
    }
}