namespace CliFx
{
    using System;

    /// <summary>
    /// Command exit message options.
    /// </summary>
    [Flags]
    public enum CommandExitMessageOptions
    {
        /// <summary>
        /// Never print exit codes.
        /// </summary>
        Never = 0,

        /// <summary>
        /// Print on error (exit code != 0).
        /// </summary>
        OnError = 1,

        /// <summary>
        /// Print on success (exit code == 0).
        /// </summary>
        OnSuccess = 2,

        /// <summary>
        /// Print in normal mode.
        /// </summary>
        InNormalMode = 4,

        /// <summary>
        /// Print in interactive mode.
        /// </summary>
        InIteractiveMode = 8,

        /// <summary>
        /// Always print exit codes.
        /// </summary>
        Always = OnError | OnSuccess | InNormalMode | InIteractiveMode,
    }
}