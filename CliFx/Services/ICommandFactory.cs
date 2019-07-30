using System;

namespace CliFx.Services
{
    public interface ICommandFactory
    {
        ICommand CreateCommand(Type commandType);
    }
}