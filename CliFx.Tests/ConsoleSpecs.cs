using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using CliWrap;
using CliWrap.Buffered;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class ConsoleSpecs : SpecsBase
    {
        public ConsoleSpecs(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        [Fact]
        public async Task Real_console_maps_directly_to_system_console()
        {
            // Can't verify console output of ourselves, so using
            // an external process for this test.

            // Arrange
            var command = "Hello world" | Cli.Wrap("dotnet")
                .WithArguments(a => a
                    .Add(Dummy.Program.Location)
                    .Add("console-test"));

            // Act
            var result = await command.ExecuteBufferedAsync();

            // Assert
            result.StandardOutput.Trim().Should().Be("Hello world");
            result.StandardError.Trim().Should().Be("Hello world");
        }

        [Fact]
        public async Task Fake_console_does_not_leak_to_system_console()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{   
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.ResetColor();
        console.ForegroundColor = ConsoleColor.DarkMagenta;
        console.BackgroundColor = ConsoleColor.DarkMagenta;
        console.CursorLeft = 42;
        console.CursorTop = 24;
        
        return default;
    }
}
");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                Array.Empty<string>(),
                new Dictionary<string, string>()
            );

            // Assert
            exitCode.Should().Be(0);

            Console.OpenStandardInput().Should().NotBe(FakeConsole.Input.BaseStream);
            Console.OpenStandardOutput().Should().NotBe(FakeConsole.Output.BaseStream);
            Console.OpenStandardError().Should().NotBe(FakeConsole.Error.BaseStream);

            Console.ForegroundColor.Should().NotBe(ConsoleColor.DarkMagenta);
            Console.BackgroundColor.Should().NotBe(ConsoleColor.DarkMagenta);

            // This fails because tests don't spawn a console window
            //Console.CursorLeft.Should().NotBe(42);
            //Console.CursorTop.Should().NotBe(24);
        }

        [Fact]
        public async Task Fake_console_can_be_used_with_an_in_memory_backing_store()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{   
    public ValueTask ExecuteAsync(IConsole console)
    {
        var input = console.Input.ReadToEnd();
        console.Output.WriteLine(input);
        console.Error.WriteLine(input);
        
        return default;
    }
}
");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            FakeConsole.WriteInput("Hello world");

            var exitCode = await application.RunAsync(
                Array.Empty<string>(),
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();
            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be("Hello world");
            stdErr.Trim().Should().Be("Hello world");
        }
    }
}