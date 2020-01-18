﻿using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
    [Command("arg cmd", Description = "Command using positional arguments")]
    public class ArgumentCommand : ICommand
    {
        [CommandParameter(0, Name = "first")]
        public string? FirstArgument { get; set; }

        [CommandParameter(10)]
        public int? SecondArgument { get; set; }

        [CommandParameter(20, Description = "A list of numbers", Name = "third list")]
        public IEnumerable<int> ThirdArguments { get; set; }

        [CommandOption("option", 'o')]
        public string Option { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}