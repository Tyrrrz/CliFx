using System;
using System.IO;
using CliFx.Services;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests.Services
{
    [TestFixture]
    public class VirtualConsoleTests
    {
        [Test]
        public void All_Smoke_Test()
        {
            // Arrange
            using (var stdin = new StringReader("hello world"))
            using (var stdout = new StringWriter())
            using (var stderr = new StringWriter())
            {
                var console = new VirtualConsole(stdin, stdout, stderr);

                // Act
                console.ResetColor();
                console.ForegroundColor = ConsoleColor.DarkMagenta;
                console.BackgroundColor = ConsoleColor.DarkMagenta;

                // Assert
                console.Input.Should().BeSameAs(stdin);
                console.IsInputRedirected.Should().BeTrue();
                console.Output.Should().BeSameAs(stdout);
                console.IsOutputRedirected.Should().BeTrue();
                console.Error.Should().BeSameAs(stderr);
                console.IsErrorRedirected.Should().BeTrue();
            }
        }
    }
}