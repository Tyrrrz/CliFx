using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Directives
{
    /// <summary>
    /// If application rans in interactive mode, this [..] directive can be used to reset current scope to default (global scope).
    /// <example>
    ///             > [>] cmd1 sub
    ///     cmd1 sub> list
    ///     cmd1 sub> [..]
    ///             >
    /// </example>
    /// </summary>
    [Directive("..", Description = "Resets the scope to default value.", InteractiveModeOnly = true)]
    internal sealed class ScopeResetDirective : IDirective
    {
        private readonly CliContext _cliContext;

        /// <inheritdoc/>
        public bool ContinueExecution => false;

        public ScopeResetDirective(ICliContext cliContext)
        {
            _cliContext = (CliContext)cliContext;
        }

        /// <inheritdoc/>
        public ValueTask HandleAsync(IConsole console)
        {
            _cliContext.Scope = string.Empty;

            return default;
        }
    }
}
