using System;
using System.Threading.Tasks;
using CliFx.Models;

namespace CliFx
{
    public abstract class Command : ICommand
    {
        public virtual ExitCode Execute() => throw new InvalidOperationException(
            "Can't execute command because its execution method is not defined. " +
            $"Override Execute or ExecuteAsync on {GetType().Name} in order to make it executable.");

        public virtual Task<ExitCode> ExecuteAsync() => Task.FromResult(Execute());
    }
}