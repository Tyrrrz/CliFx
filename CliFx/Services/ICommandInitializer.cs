using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Populates <see cref="ICommand"/> instances with input according to its schema.
    /// </summary>
    public interface ICommandInitializer
    {
        /// <summary>
        /// Populates an instance of <see cref="ICommand"/> with specified input according to specified schema.
        /// </summary>
        void InitializeCommand(ICommand command, CommandSchema schema, CommandInput input);
    }
}