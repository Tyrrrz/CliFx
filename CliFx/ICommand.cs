using System.Threading.Tasks;
using CliFx.Infrastructure;
using CliFx.Input;
using CliFx.Schema;

namespace CliFx;

/// <summary>
/// Entry point through which the user interacts with the command-line application.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Binds the command input to the current instance, using the provided schema.
    /// </summary>
    /// <remarks>
    /// This method is implemented automatically by the framework and should not be
    /// called directly.
    /// </remarks>
    void Bind(CommandSchema schema, CommandInput input);

    /// <summary>
    /// Executes the command using the specified implementation of <see cref="IConsole" />.
    /// </summary>
    /// <remarks>
    /// If the execution of the command is not asynchronous, simply end the method with
    /// <c>return default;</c>
    /// </remarks>
    ValueTask ExecuteAsync(IConsole console);
}
