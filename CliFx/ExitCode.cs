using System;
using CliFx.Exceptions;

namespace CliFx
{
    internal static class ExitCode
    {
        public const int Success = 0;

        public static int FromException(Exception ex) => ex switch
        {
            CommandException cmdEx => cmdEx.ExitCode,
            _ => 1
        };
    }
}