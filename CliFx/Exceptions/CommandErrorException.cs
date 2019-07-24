using System;

namespace CliFx.Exceptions
{
    public class CommandErrorException : Exception
    {
        public int ExitCode { get; }

        public CommandErrorException(int exitCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ExitCode = exitCode;
        }

        public CommandErrorException(int exitCode, Exception innerException)
            : this(exitCode, null, innerException)
        {
        }

        public CommandErrorException(int exitCode, string message)
            : this(exitCode, message, null)
        {
        }

        public CommandErrorException(int exitCode)
            : this(exitCode, null, null)
        {
        }
    }
}