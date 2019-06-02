using System;

namespace CliFx.Services
{
    public interface ICommandOptionConverter
    {
        object ConvertOption(string value, Type targetType);
    }
}