using NUnit.Framework;
using System;
using System.IO;
using CliFx.Services;
using CliFx.Tests.Stubs;
using CliFx.Tests.TestCommands;

namespace CliFx.Tests
{
    [TestFixture]
    public class CliApplicationBuilderTests
    {
        // Make sure all builder methods work
        [Test]
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
                .UseCommandOptionInputConverter(new CommandOptionInputConverter())
                .UseEnvironmentVariablesProvider(new EnvironmentVariablesProviderStub())
                .Build();
        }

        // Make sure builder can produce an application with no parameters specified
        [Test]
        public void Build_Test()
        {
            // Arrange
            var builder = new CliApplicationBuilder();

            // Act
            builder.Build();
        }
    }
}