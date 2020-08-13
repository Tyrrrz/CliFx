using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Directives
{
    /// <summary>
    /// If application rans in interactive mode, [>] directive followed by command(s) would scope to the command(s), allowing to ommit specified command name(s).
    /// <example>
    /// Commands:
    ///              > [>] cmd1 sub
    ///      cmd1 sub> list
    ///      cmd1 sub> get
    ///              > [>] cmd1
    ///          cmd1> test
    ///          cmd1> -h
    ///
    /// are an equivalent to:
    ///              > cmd1 sub list
    ///              > cmd1 sub get
    ///              > cmd1 test
    ///              > cmd1 -h
    /// </example>
    /// </summary>
    [Directive(">", Description = "Sets a scope to command(s).", InteractiveModeOnly = true)]
    internal sealed class ScopeDirective : IDirective
    {
        private readonly CliContext _cliContext;

        /// <inheritdoc/>
        public bool ContinueExecution => false;

        public ScopeDirective(ICliContext cliContext)
        {
            _cliContext = (CliContext)cliContext;
        }

        /// <inheritdoc/>
        public ValueTask HandleAsync(IConsole console)
        {
            _cliContext.Scope = _cliContext.CurrentInput.CommandName ?? string.Empty;

            return default;
        }
    }
}
