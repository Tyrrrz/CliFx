using System;

namespace CliFx.Services
{
    /// <summary>
    /// Initializes new instances of <see cref="ICommand"/>.
    /// </summary>
    public interface ICommandFactory
    {
        /// <summary>
        /// Initializes an instance of <see cref="ICommand"/> of specified type.
        /// </summary>
        ICommand CreateCommand(Type commandType);
    }
}