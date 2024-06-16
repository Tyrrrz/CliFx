using CliFx.Input;

namespace CliFx;

/// <summary>
/// Command whose inputs can be bound from command-line arguments.
/// </summary>
/// <remarks>
/// This interface is required to facilitate binding of command inputs (parameters and options)
/// to their corresponding CLR properties.
/// You should not need to implement this interface directly, as it will be automatically
/// implemented by the framework.
/// </remarks>
public interface IBindableCommand : ICommand
{
    /// <summary>
    /// Binds the command input to the current instance.
    /// </summary>
    void Bind(CommandInput input);
}