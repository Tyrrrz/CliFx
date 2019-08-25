using System;
using System.IO;
using CliFx.Services;
using CliFx.Tests.TestCommands;
using NUnit.Framework;

namespace CliFx.Tests
{
    [TestFixture]
    public class CliApplicationBuilderTests
    {
        // Make sure all builder methods work
        [Test]
        public void Build_Smoke_Test()
        {
            // Arrange
            var builder = new CliApplicationBuilder();

            // Act
            builder
                .AddCommand(typeof(EchoCommand))
                .AddCommandsFrom(typeof(EchoCommand).Assembly)
                .AddCommands(new[] {typeof(EchoCommand)})
                .AddCommandsFrom(new[] {typeof(EchoCommand).Assembly})
                .AddCommandsFromThisAssembly()
                .AllowDebugMode()
                .AllowPreviewMode()
                .UseTitle("test")
                .UseExecutableName("test")
                .UseVersionText("test")
                .UseDescription("test")
                .UseConsole(new VirtualConsole(TextWriter.Null))
                .UseCommandFactory(schema => (ICommand) Activator.CreateInstance(schema.Type))
                .Build();
        }

        // Make sure builder can produce a default application
        [Test]
        public void Build_Fallback_Smoke_Test()
        {
            // Arrange
            var builder = new CliApplicationBuilder();

            // Act
            builder.Build();
        }
    }
}