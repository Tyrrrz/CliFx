using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Directives
{
    [Directive("scope")]
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
