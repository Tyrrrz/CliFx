using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Directives
{
    [Directive("..")]
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
