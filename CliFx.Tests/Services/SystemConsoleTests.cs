using System;
using CliFx.Services;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests.Services
{
    [TestFixture]
    public class SystemConsoleTests
    {
        [TearDown]
        public void TearDown()
        {
            // Reset console color so it doesn't carry on into next tests
            Console.ResetColor();
        }

        [Test(Description = "Synchronized with system console")]
        public void All_Smoke_Test()
        {
            // Arrange
            var console = new SystemConsole();

            // Act
            console.ResetColor();
            console.ForegroundColor = ConsoleColor.DarkMagenta;
            console.BackgroundColor = ConsoleColor.DarkMagenta;

            // Assert
            console.Input.Should().BeSameAs(Console.In);
            console.IsInputRedirected.Should().Be(Console.IsInputRedirected);
            console.Output.Should().BeSameAs(Console.Out);
            console.IsOutputRedirected.Should().Be(Console.IsOutputRedirected);
            console.Error.Should().BeSameAs(Console.Error);
            console.IsErrorRedirected.Should().Be(Console.IsErrorRedirected);
            console.ForegroundColor.Should().Be(Console.ForegroundColor);
            console.BackgroundColor.Should().Be(Console.BackgroundColor);
        }
    }
}