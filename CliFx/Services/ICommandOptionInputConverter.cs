using System;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Converts input command options.
    /// </summary>
    public interface ICommandOptionInputConverter
    {
        /// <summary>
        /// Converts an option to specified target type.
        /// </summary>
        object ConvertOption(CommandOptionInput option, Type targetType);
    }
}