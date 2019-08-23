using System;
using CliFx.Services;

namespace CliFx.Utilities
{
    /// <summary>
    /// Utility that provides continuous progress reporting to the console.
    /// </summary>
    public class ProgressReporter : IProgress<double>
    {
        private readonly IConsole _console;

        private string _lastOutput = "";

        /// <summary>
        /// Initializes an instance of <see cref="ProgressReporter"/>.
        /// </summary>
        public ProgressReporter(IConsole console)
        {
            _console = console;
        }

        private void EraseLastOutput()
        {
            for (var i = 0; i < _lastOutput.Length; i++)
                _console.Output.Write('\b');
        }

        private void RenderProgress(double progress)
        {
            _lastOutput = progress.ToString("P2", _console.Output.FormatProvider);
            _console.Output.Write(_lastOutput);
        }

        /// <summary>
        /// Reports progress and renders it to console.
        /// If console's stdout is redirected, this method returns without doing anything.
        /// </summary>
        public void Report(double progress)
        {
            // We don't do anything if stdout is redirected to avoid polluting output
            //...when there's no active console window.
            if (!_console.IsOutputRedirected)
            {
                EraseLastOutput();
                RenderProgress(progress);
            }
        }
    }
}