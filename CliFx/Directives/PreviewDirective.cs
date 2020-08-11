using System;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Domain.Input;

namespace CliFx.Directives
{
    /// <summary>
    /// When troubleshooting issues, you may find it useful to run your app in preview mode.
    /// If preview mode is specified (using the `[preview]` directive), the app will short-circuit by printing consumed command line arguments as they were parsed.
    /// This is useful when troubleshooting issues related to command routing and argument binding.
    /// </summary>
    [Directive("preview")]
    public sealed class PreviewDirective : IDirective
    {
        private readonly ICliContext _cliContext;

        /// <inheritdoc/>
        public bool ContinueExecution => false;

        /// <summary>
        /// Initializes an instance of <see cref="CliApplication"/>.
        /// </summary>
        public PreviewDirective(ICliContext cliContext) //TODO: use Microsoft.Extensions.depenedcyInjection by default or resolve ICliContext
        {
            _cliContext = cliContext;
        }

        /// <inheritdoc/>
        public ValueTask HandleAsync(IConsole console)
        {
            WriteCommandLineInput(console, _cliContext.CurrentInput);

            return default;
        }

        private void WriteCommandLineInput(IConsole console, CommandInput input)
        {
            // Command name
            if (!string.IsNullOrWhiteSpace(input.CommandName))
            {
                console.WithForegroundColor(ConsoleColor.Cyan, () =>
                    console.Output.Write(input.CommandName));

                console.Output.Write(' ');
            }

            // Parameters
            foreach (var parameter in input.Parameters)
            {
                console.Output.Write('<');

                console.WithForegroundColor(ConsoleColor.White, () =>
                    console.Output.Write(parameter));

                console.Output.Write('>');
                console.Output.Write(' ');
            }

            // Options
            foreach (var option in input.Options)
            {
                console.Output.Write('[');

                console.WithForegroundColor(ConsoleColor.White, () =>
                {
                    // Alias
                    console.Output.Write(option.GetRawAlias());

                    // Values
                    if (option.Values.Any())
                    {
                        console.Output.Write(' ');
                        console.Output.Write(option.GetRawValues());
                    }
                });

                console.Output.Write(']');
                console.Output.Write(' ');
            }

            console.Output.WriteLine();
        }
    }
}
