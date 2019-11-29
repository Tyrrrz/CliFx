using System;
using System.Collections.Generic;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Converts input command options.
    /// </summary>
    public interface ICommandInputConverter
    {
        /// <summary>
        /// Converts an option to specified target type.
        /// </summary>
        object? ConvertOptionInput(CommandOptionInput optionInput, Type targetType);

        /// <summary>
        /// Converts an argument to specified target type, using up arguments from the given enumerator.
        /// </summary>
        object? ConvertArgumentInput(IEnumerator<string> argumentEnumerator, Type targetType);
    }
}