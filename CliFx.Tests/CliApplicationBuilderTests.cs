using NUnit.Framework;
using System;
using System.IO;
using CliFx.Tests.TestCommands;

namespace CliFx.Tests
{
    [TestFixture]
    public class CliApplicationBuilderTests
    {
        [Test(Description = "All builder methods must return without exceptions")]
        public void Smoke_Test()
        {
            // Arrange
            var builder = new CliApplicationBuilder();

            // Act
            builder
                .AddCommand(typeof(HelloWorldDefaultCommand))
                .AddCommandsFrom(typeof(HelloWorldDefaultCommand).Assembly)
                .AddCommands(new[] {typeof(HelloWorldDefaultCommand)})
                .AddCommandsFrom(new[] {typeof(HelloWorldDefaultCommand).Assembly})
                .AddCommandsFromThisAssembly()
                .AllowDebugMode()
                .AllowPreviewMode()
                .UseTitle("test")
                .UseExecutableName("test")
                .UseVersionText("test")
                .UseDescription("test")
                .UseConsole(new VirtualConsole(TextWriter.Null))
                .UseTypeActivator(Activator.CreateInstance)
                .Build();
        }

        [Test(Description = "Builder must produce an application when no parameters were specified")]
        public void Build_Test()
        {
            // Arrange
            var builder = new CliApplicationBuilder();

            // Act
            builder.Build();
        }
    }
}