using System;
using CliFx.Models;

namespace CliFx.Services
{
    public interface ICommandOptionInputConverter
    {
        object ConvertOption(CommandOptionInput option, Type targetType);
    }
}