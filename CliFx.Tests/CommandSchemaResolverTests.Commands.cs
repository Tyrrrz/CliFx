using System;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests
{
    public partial class CommandSchemaResolverTests
    {
        [Command("cmd", Description = "NormalCommand1 description.")]
        private class NormalCommand1 : ICommand
        {
            [CommandOption("option-a", 'a')]
            public int OptionA { get; set; }

            [CommandOption("option-b", IsRequired = true)]
            public string OptionB { get; set; }

            public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
        }

        [Command(Description = "NormalCommand2 description.")]
        private class NormalCommand2 : ICommand
        {
            [CommandOption("option-c", Description = "OptionC description.")]
            public bool OptionC { get; set; }

            [CommandOption("option-d", 'd')]
            public DateTimeOffset OptionD { get; set; }

            public string NotAnOption { get; set; }

            public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
        }
    }

    // Negative
    public partial class CommandSchemaResolverTests
    {
        [Command("conflict")]
        private class ConflictingCommand1 : ICommand
        {
            public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
        }

        [Command("conflict")]
        private class ConflictingCommand2 : ICommand
        {
            public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
        }

        [Command]
        private class InvalidCommand1
        {
        }

        // No attribute
        private class InvalidCommand2 : ICommand
        {
            public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
        }

        [Command]
        private class InvalidCommand3 : ICommand
        {
            [CommandOption("conflict")]
            public string ConflictingOption1 { get; set; }

            [CommandOption("conflict")]
            public string ConflictingOption2 { get; set; }

            public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
        }

        [Command]
        private class InvalidCommand4 : ICommand
        {
            [CommandOption('c')]
            public string ConflictingOption1 { get; set; }

            [CommandOption('c')]
            public string ConflictingOption2 { get; set; }

            public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
        }
    }
}