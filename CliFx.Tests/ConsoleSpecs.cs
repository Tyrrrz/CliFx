using System;
using System.Threading.Tasks;
using CliFx.Infrastructure;
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
        public async Task Real_implementation_of_console_maps_directly_to_system_console()
        {
            // Arrange
            var command = "Hello world" | Cli.Wrap("dotnet")
                .WithArguments(a => a
                    .Add(Dummy.Program.Location)
                    .Add("console-test"));

            // Act
            var result = await command.ExecuteBufferedAsync();

            // Assert
            result.StandardOutput.TrimEnd().Should().Be("Hello world");
            result.StandardError.TrimEnd().Should().Be("Hello world");
        }

        [Fact]
        public void Fake_implementation_of_console_does_not_leak_to_real_console()
        {
            // TODO: test at higher level?

            // Arrange
            using var console = new FakeInMemoryConsole();

            // Act
            console.WriteInput("input");
            console.Output.Write("output");
            console.Error.Write("error");

            var stdIn = console.Input.ReadToEnd();
            var stdOut = console.ReadOutputString();
            var stdErr = console.ReadErrorString();

            console.ResetColor();
            console.ForegroundColor = ConsoleColor.DarkMagenta;
            console.BackgroundColor = ConsoleColor.DarkMagenta;
            console.CursorLeft = 42;
            console.CursorTop = 24;

            // Assert
            stdIn.Should().Be("input");
            stdOut.Should().Be("output");
            stdErr.Should().Be("error");

            console.Input.Should().NotBeSameAs(Console.In);
            console.Output.Should().NotBeSameAs(Console.Out);
            console.Error.Should().NotBeSameAs(Console.Error);

            console.IsInputRedirected.Should().BeTrue();
            console.IsOutputRedirected.Should().BeTrue();
            console.IsErrorRedirected.Should().BeTrue();

            console.ForegroundColor.Should().NotBe(Console.ForegroundColor);
            console.BackgroundColor.Should().NotBe(Console.BackgroundColor);
        }
    }
}