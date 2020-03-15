using System;
using System.IO;
using System.Threading.Tasks;
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
            using var stdIn = new MemoryStream(Console.InputEncoding.GetBytes("input"));
            using var stdOut = new MemoryStream();
            using var stdErr = new MemoryStream();

            var console = new VirtualConsole(
                input: stdIn,
                output: stdOut,
                error: stdErr);

            // Act
            console.Output.Write("output");
            console.Error.Write("error");

            var stdInData = console.Input.ReadToEnd();
            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray());
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray());

            console.ResetColor();
            console.ForegroundColor = ConsoleColor.DarkMagenta;
            console.BackgroundColor = ConsoleColor.DarkMagenta;

            // Assert
            stdInData.Should().Be("input");
            stdOutData.Should().Be("output");
            stdErrData.Should().Be("error");

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