using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class SuggestDirectivesSpecs : SpecsBase
    {
        public SuggestDirectivesSpecs(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        private string _cmdCommandCs = @"
[Command(""cmd"")]
public class Command : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
}
";

        public CliApplicationBuilder TestApplicationFactory(params string[] commandClasses)
        {
            var builder = new CliApplicationBuilder();
            
            commandClasses.ToList().ForEach(c =>
            {
                var commandType = DynamicCommandBuilder.Compile(c);
                builder = builder.AddCommand(commandType);
            });

            return builder.UseConsole(FakeConsole);
        }

        [Theory]
        [InlineData(true, 0 )]
        [InlineData(false, 1)]
        public async Task Suggest_directive_can_be_configured(bool enabled, int expectedExitCode)
        {
            // Arrange
            var application = TestApplicationFactory(_cmdCommandCs)
                .AllowSuggestMode(enabled)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] { "[suggest]", "clifx.exe", "c" }
            );

            // Assert
            exitCode.Should().Be(expectedExitCode);
        }

        [Fact]
        public async Task Suggest_directive_is_enabled_by_default()
        {
            // Arrange
            var application = TestApplicationFactory(_cmdCommandCs)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] { "[suggest]", "clifx.exe", "c" }
            );

            // Assert
            exitCode.Should().Be(0);
        }
    }
}