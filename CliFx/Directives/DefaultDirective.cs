namespace CliFx.Directives
{
    using System.Threading.Tasks;
    using CliFx.Attributes;

    /// <summary>
    /// Normally if application rans in interactive mode, an empty line does nothing; but [default] will override this behaviour, executing a root (empty) command or scoped command without arguments.
    /// </summary>
    [Directive("default", Description = "Executes a root (empty) command or scoped command without arguments (parameters and options).", InteractiveModeOnly = true)]
    public sealed class DefaultDirective : IDirective
    {
        private readonly ICliContext _cliContext;

        /// <inheritdoc/>
        public bool ContinueExecution { get; private set; }

        /// <summary>
        /// Initializes an instance of <see cref="DefaultDirective"/>.
        /// </summary>
        public DefaultDirective(ICliContext cliContext)
        {
            _cliContext = cliContext;
        }

        /// <inheritdoc/>
        public ValueTask HandleAsync(IConsole console)
        {
            //TODO: maybe make [default] -h etc forbidden
            //bool isInteractiveMode = _cliContext.IsInteractiveMode;
            //string scope = _cliContext.Scope;
            //CommandInput input = _cliContext.CurrentInput;

            // if (input.IsDefaultCommandOrEmpty)
            ContinueExecution = true;

            return default;
        }
    }
}
