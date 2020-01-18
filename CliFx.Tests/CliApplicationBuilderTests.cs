using NUnit.Framework;
using System;
using System.IO;
using CliFx.Services;
using CliFx.Tests.TestCommands;

namespace CliFx.Tests
{
    [TestFixture]
    public class CliApplicationBuilderTests
    {
        // Make sure all builder methods work
        [Test(Description = "All builder methods return without exceptions")]
        public void All_Smoke_Test()
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
                .UseCommandFactory(schema => (ICommand) Activator.CreateInstance(schema.Type!)!)
                .Build();
        }

        [Test(Description = "Builder can produce an application when no parameters were specified")]
        public void Build_Test()
        {
            // Arrange
            var builder = new CliApplicationBuilder();

            // Act
            builder.Build();
        }
    }
}