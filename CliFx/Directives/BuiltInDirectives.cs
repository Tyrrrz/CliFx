namespace CliFx
{
    /// <summary>
    /// Standard command directives definitions.
    /// </summary>
    public static class BuiltInDirectives
    {
        /// <summary>
        /// When application is ran in debug mode (using the [debug] directive), it will wait for debugger to be attached before proceeding.
        /// This is useful for debugging apps that were ran outside of the IDE.
        /// </summary>
        public const string Debug = "debug";

        /// <summary>
        /// When preview mode is specified (using the [preview] directive), the app will short-circuit by printing consumed command line arguments as they were parsed.
        /// This is useful when troubleshooting issues related to command routing and argument binding.
        /// </summary>
        public const string Preview = "preview";

        /// <summary>
        /// If application rans in interactive mode (using the [interactive] directive), it is possible to execute multiple commands in one processes.
        /// The application will ran in loop, constantly asking user for command input.
        /// This is useful for situations when it is necessary to execute multiple commands (since you don't have to constantly type dotnet ...).
        /// Furthermore, application context can be shared, which is useful when you have a db connection or startup takes very long.
        /// </summary>
        public const string Interactive = "interactive";

        /// <summary>
        /// Normally if application rans in interactive mode, an empty line does nothing; but [default] will override this behaviour, executing a root (empty) command or scoped command.
        /// </summary>
        public const string Default = "default";

        /// <summary>
        /// If application rans in interactive mode, [>] directive followed by command(s) name(s) would scope to the command(s), allowing to ommit specified command name(s).
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
        public const string Scope = ">";

        /// <summary>
        /// If application rans in interactive mode, [.] directive can be used to remove one command from the scope.
        /// <example>
        ///             > [>] cmd1 sub
        ///     cmd1 sub> list
        ///     cmd1 sub> [.]
        ///         cmd1>
        /// </example>
        /// </summary>
        public const string ScopeUp = ".";

        /// <summary>
        /// If application rans in interactive mode, [..] directive can be used to reset current scope to default (global scope).
        /// <example>
        ///             > [>] cmd1 sub
        ///     cmd1 sub> list
        ///     cmd1 sub> [..]
        ///             >
        /// </example>
        /// </summary>
        public const string ScopeReset = "..";
    }
}
