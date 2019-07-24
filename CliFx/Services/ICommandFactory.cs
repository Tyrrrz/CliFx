using System;
using CliFx.Models;

namespace CliFx.Services
{
    public interface ICommandFactory
    {
        ICommand CreateCommand(CommandSchema schema);
    }
}