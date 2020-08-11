using System;
using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Directives
{
    [Directive(".")]
    internal sealed class ScopeUpDirective : IDirective
    {
        private readonly CliContext _cliContext;

        /// <inheritdoc/>
        public bool ContinueExecution => false;

        public ScopeUpDirective(ICliContext cliContext)
        {
            _cliContext = (CliContext)cliContext;
        }

        /// <inheritdoc/>
        public ValueTask HandleAsync(IConsole console)
        {
            // Scope up
            if (_cliContext.CurrentInput.HasDirective(StandardDirectives.ScopeUp))
            {
                string[] splittedScope = _cliContext.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (splittedScope.Length > 1)
                    _cliContext.Scope = string.Join(" ", splittedScope, 0, splittedScope.Length - 1);
                else if (splittedScope.Length == 1)
                    _cliContext.Scope = string.Empty;
            }

            return default;
        }
    }
}
