using System;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests
{
    public class VirtualConsoleSpecs
    {
        [Fact]
        public void Fake_implementation_of_console_can_be_used_to_execute_commands_in_isolation()
        {
            // Arrange
            using var console = new VirtualConsole();

            // Act
            console.WriteInputString("input");
            var consumedInput = console.Input.ReadToEnd();

            console.Output.Write("output");
            var printedOutput = console.ReadOutputString();

            console.Error.Write("error");
            var printedError = console.ReadErrorString();

            console.ResetColor();
            console.ForegroundColor = ConsoleColor.DarkMagenta;
            console.BackgroundColor = ConsoleColor.DarkMagenta;

            // Assert
            consumedInput.Should().Be("input");
            printedOutput.Should().Be("output");
            printedError.Should().Be("error");

            console.Input.Should().NotBeSameAs(Console.In);
            console.Output.Should().NotBeSameAs(Console.Out);
            console.Error.Should().NotBeSameAs(Console.Error);

            console.ForegroundColor.Should().NotBe(Console.ForegroundColor);
            console.BackgroundColor.Should().NotBe(Console.BackgroundColor);
        }
    }
}