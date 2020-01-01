using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Initializes new instances of <see cref="ICommand"/>.
    /// </summary>
    public interface ICommandFactory
    {
        /// <summary>
        /// Initializes an instance of <see cref="ICommand"/> with specified schema.
        /// </summary>
        ICommand CreateCommand(ICommandSchema commandSchema);
    }
}