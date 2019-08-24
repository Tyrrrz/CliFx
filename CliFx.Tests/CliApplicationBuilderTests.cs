using System;
using System.IO;
using CliFx.Services;
using NUnit.Framework;

namespace CliFx.Tests
{
    [TestFixture]
    public partial class CliApplicationBuilderTests
    {
        [Test]
        public void Build_Smoke_Test()
        {
            // Just test that application can be built after calling all methods

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
    }
}