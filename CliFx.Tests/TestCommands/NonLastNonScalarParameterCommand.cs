﻿using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests.TestCommands
{
    [Command]
    public class NonLastNonScalarParameterCommand : ICommand
    {
        [CommandParameter(0)]
        public IReadOnlyList<string>? ParameterA { get; set; }

        [CommandParameter(1)]
        public string? ParameterB { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}