using System;
using CliFx.Models;

namespace CliFx.Services
{
    public interface ICommandOptionConverter
    {
        object ConvertOption(CommandOption option, Type targetType);
    }
}