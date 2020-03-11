using System;
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
            using var console = new VirtualConsole();
            console.WriteInputString("hello world");

            // Act
            console.ResetColor();
            console.ForegroundColor = ConsoleColor.DarkMagenta;
            console.BackgroundColor = ConsoleColor.DarkMagenta;

            // Assert
            console.Input.Should().NotBeSameAs(Console.In);
            console.IsInputRedirected.Should().BeTrue();
            console.Output.Should().NotBeSameAs(Console.Out);
            console.IsOutputRedirected.Should().BeTrue();
            console.Error.Should().NotBeSameAs(Console.Error);
            console.IsErrorRedirected.Should().BeTrue();
            console.ForegroundColor.Should().NotBe(Console.ForegroundColor);
            console.BackgroundColor.Should().NotBe(Console.BackgroundColor);
        }
    }
}