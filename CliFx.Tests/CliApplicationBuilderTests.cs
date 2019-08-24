using System;
using System.IO;
using CliFx.Services;
using NUnit.Framework;

namespace CliFx.Tests
{
    [TestFixture]
    public partial class CliApplicationBuilderTests
    {
        // Make sure all builder methods work
        [Test]
        public void Build_Smoke_Test()
        {
            // Arrange
            var builder = new CliApplicationBuilder();

            // Act
            builder
                .AddCommand(typeof(TestCommand))
                .AddCommandsFrom(typeof(TestCommand).Assembly)
                .AddCommands(new[] {typeof(TestCommand)})
                .AddCommandsFrom(new[] {typeof(TestCommand).Assembly})
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