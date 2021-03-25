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

        private string _cmd2CommandCs = @"
[Command(""cmd02"")]
public class Command02 : ICommand
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

        private string FormatExpectedOutput(string [] s)
        {
            if( s.Length == 0)
            {
                return "";
            }
            return string.Join("\r\n", s) + "\r\n";
        }

        [Theory]
        [InlineData("supply all commands if nothing supplied",                    
                        "clifx.exe", new[] { "cmd", "cmd02" })]
        [InlineData("supply all commands that match partially",
                        "clifx.exe c", new[] { "cmd", "cmd02" })]
        [InlineData("supply command options if match found, regardles of other partial matches (no options defined)",
                        "clifx.exe cmd", new string[] { })]
        [InlineData("supply nothing if no partial match applies",
                        "clifx.exe cmd2", new string[] { })]
        public async Task Suggest_directive_accepts_command_line_by_environment_variable(string usecase, string variableContents, string[] expected)
        {
            // Arrange
            var application = TestApplicationFactory(_cmdCommandCs, _cmd2CommandCs)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] { "[suggest]", "--envvar", "CLIFX-{GUID}", "--cursor", variableContents.Length.ToString() },
                new Dictionary<string, string>()
                {
                    ["CLIFX-{GUID}"] = variableContents
                }
            );

            var stdOut = FakeConsole.ReadOutputString();
            
            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().Be(FormatExpectedOutput(expected), usecase);
        }

        [Theory]
        [InlineData("supply all commands that match partially",
                        new[] { "[suggest]", "clifx.exe", "c" }, new[] { "cmd", "cmd02" })]
        [InlineData("supply command options if match found, regardles of other partial matches (no options defined)",
                        new[] { "[suggest]", "clifx.exe", "cmd" }, new string[] { })]
        public async Task Suggest_directive_suggests_commands_by_command_line_only(string usecase, string[] commandLine, string[] expected)
        {
            // Arrange
            var application = TestApplicationFactory(_cmdCommandCs, _cmd2CommandCs)
                .Build();

            // Act
            var exitCode = await application.RunAsync(commandLine);

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().Be(FormatExpectedOutput(expected), usecase);
        }

        //[Theory]
        //[InlineData("happy case", "clifx.exe c", "")]
        //public async Task Suggest_directive_generates_suggestions(string because, string commandline, string expectedResult)
        //{
        //    // Arrange
        //    var application = TestApplicationFactory(_cmdCommandCs)
        //        .Build();

        //    // Act
        //    var exitCode = await application.RunAsync(
        //        new[] { "[suggest]", commandline }
        //    );

        //    var stdOut = FakeConsole.ReadOutputString();

        //    // Assert
        //    exitCode.Should().Be(0);

        //    stdOut.Should().Be(expectedResult + "\r\n", because);
        //}
    }
}