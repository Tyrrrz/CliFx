using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Commands;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class DirectivesSpecs : SpecsBase
    {
        public DirectivesSpecs(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        [Fact]
        public async Task Preview_directive_can_be_specified_to_print_provided_arguments_as_they_were_parsed()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command(""cmd"")]
public class Command : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
}
");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .AllowPreviewMode()
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"[preview]", "cmd", "param", "-abc", "--option", "foo"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAllInOrder(
                "cmd", "<param>", "[-a]", "[-b]", "[-c]", "[--option \"foo\"]"
            );
        }
    }
}