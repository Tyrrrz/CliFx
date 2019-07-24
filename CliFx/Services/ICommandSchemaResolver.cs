using System;
using CliFx.Models;

namespace CliFx.Services
{
    public interface ICommandSchemaResolver
    {
        CommandSchema GetCommandSchema(Type commandType);
    }
}