using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests
{
    [TestFixture]
    public class VirtualConsoleTests
    {
        [Test(Description = "Must not leak to system console")]
        public void Smoke_Test()
        {
            // Arrange
            using var stdin = new StringReader("hello world");
            using var stdout = new StringWriter();
            using var stderr = new StringWriter();

            var console = new VirtualConsole(stdin, stdout, stderr);

            // Act
            console.ResetColor();
            console.ForegroundColor = ConsoleColor.DarkMagenta;
            console.BackgroundColor = ConsoleColor.DarkMagenta;

            // Assert
            console.Input.Should().BeSameAs(stdin);
            console.Input.Should().NotBeSameAs(Console.In);
            console.IsInputRedirected.Should().BeTrue();
            console.Output.Should().BeSameAs(stdout);
            console.Output.Should().NotBeSameAs(Console.Out);
            console.IsOutputRedirected.Should().BeTrue();
            console.Error.Should().BeSameAs(stderr);
            console.Error.Should().NotBeSameAs(Console.Error);
            console.IsErrorRedirected.Should().BeTrue();
            console.ForegroundColor.Should().NotBe(Console.ForegroundColor);
            console.BackgroundColor.Should().NotBe(Console.BackgroundColor);
        }
    }
}