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

        private string _parentCommandCs = @"
[Command(""parent"")]
public class ParentCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
}
";

        private string _childCommandCs = @"
[Command(""parent list"")]
public class ParentCommand : ICommand
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

            return builder.UseConsole(FakeConsole)
                          .UseFileSystem(NullFileSystem);
        }

        [Theory]
        [InlineData(true, 0)]
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
        public async Task Suggest_directive_is_disabled_by_default()
        {
            // Arrange
            var application = TestApplicationFactory(_cmdCommandCs)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] { "[suggest]", "clifx.exe", "c" }
            );

            // Assert
            exitCode.Should().Be(1);
        }

        [Theory]
        [InlineData("supply all commands if nothing supplied",
                        "clifx.exe", 0, new[] { "cmd", "cmd02", "parent", "parent list" })]
        [InlineData("supply all commands that 'start with' argument",
                        "clifx.exe c", 0, new[] { "cmd", "cmd02" })]
        [InlineData("supply command options if match found, regardles of other partial matches (no options defined)",
                        "clifx.exe cmd", 0, new string[] { })]
        [InlineData("supply nothing if no commands 'starts with' argument",
                        "clifx.exe m", 0, new string[] { })]
        [InlineData("supply completions of partial child commands",
                        "clifx.exe parent l", 0, new[] { "list" })]
        [InlineData("supply all commands that 'start with' argument, allowing for cursor position",
                        "clifx.exe cmd", -2, new[] { "cmd", "cmd02" })]
        public async Task Suggest_directive_suggests_commands_by_environment_variables(string usecase, string variableContents, int cursorOffset, string[] expected)
        {
            // Arrange
            var application = TestApplicationFactory(_cmdCommandCs, _cmd2CommandCs, _parentCommandCs, _childCommandCs)
                .AllowSuggestMode()
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] { "[suggest]", "--envvar", "CLIFX-{GUID}", "--cursor", (variableContents.Length + cursorOffset).ToString() },
                new Dictionary<string, string>()
                {
                    ["CLIFX-{GUID}"] = variableContents
                }
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);

            stdOut.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                  .Should().BeEquivalentTo(expected, usecase);
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
                .AllowSuggestMode()
                .Build();

            // Act
            var exitCode = await application.RunAsync(commandLine);

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);

            stdOut.Split(null)
                  .Where(p => !string.IsNullOrWhiteSpace(p))
                  .Should().BeEquivalentTo(expected, usecase);
        }

        [Theory]
        [InlineData("suggest all option names",
                      "clifx.exe opt --", 0, new[] { "--help", "--opt", "--opt01", "--opt02" })]
        [InlineData("suggest all option names beginning with prefix",
                      "clifx.exe opt --opt0", 0, new[] { "--opt01", "--opt02" })]
        [InlineData("suggest all option names beginning with prefix that also match short names",
                      "clifx.exe opt --o", 0, new[] { "--opt", "--opt01", "--opt02" })]
        [InlineData("suggest all option names and aliases",
                      "clifx.exe opt -", 0, new[] { "-1", "-2", "-h", "-o", "--help", "--opt", "--opt01", "--opt02" })]
        [InlineData("don't suggest additional aliases because it doesn't feel right even if it is valid?",
                      "clifx.exe opt -1", 0, new string[] { })]
        [InlineData("don't suggest for exact matches",
                      "clifx.exe opt --opt01", 0, new string[] { })]
        public async Task Suggest_directive_suggests_options(string usecase, string variableContents, int cursorOffset, string[] expected)
        {
            // Arrange
            var optCommandCs = @"
[Command(""opt"")]
public class OptionCommand : ICommand
{
    [CommandOption(""opt"", 'o')]
    public string Option { get; set; } = """";

    [CommandOption(""opt01"", '1')]
    public string Option01 { get; set; } = """";

    [CommandOption(""opt02"", '2')]
    public string Option02 { get; set; } = """";

    public ValueTask ExecuteAsync(IConsole console) => default;
}
";
            var application = TestApplicationFactory(optCommandCs)
                .AllowSuggestMode()
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] { "[suggest]", "--envvar", "CLIFX-{GUID}", "--cursor", (variableContents.Length + cursorOffset).ToString() },
                new Dictionary<string, string>()
                {
                    ["CLIFX-{GUID}"] = variableContents
                }
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);

            stdOut.Split(null)
                  .Where(p => !string.IsNullOrWhiteSpace(p))
                  .Should().BeEquivalentTo(expected, usecase);
        }


        [Theory]
        [InlineData("don't suggest parameters that don't have a sensible suggestion",
                     "clifx.exe cmd x", 0, new string[] { })]
        [InlineData("suggest parameters where valid values are present",
                     "clifx.exe cmd x Re", 0, new[] { "Red", "RedOrange" })]
        [InlineData("don't suggest parameters where complete values are present",
                     "clifx.exe cmd x Red", 0, new string[] { })]
        [InlineData("suggest for non-scalar parameters",
                     "clifx.exe cmd x Red R", 0, new[] { "Red", "RedOrange" })]
        [InlineData("suggest options when parameter present",
                     "clifx.exe cmd x --opt0", 0, new[] { "--opt01", "--opt02" })]
        public async Task Suggest_directive_suggests_parameters(string usecase, string variableContents, int cursorOffset, string[] expected)
        {
            // Arrange
            var optCommandCs = @"
public enum TestColor
{
    Red, RedOrange, Green, Blue
}

[Command(""cmd"")]
public class ParameterCommand : ICommand
{
    [CommandParameter(0, Name = ""param"")]
    public string Parameter { get; set; } = """";

    [CommandParameter(1, Name = ""color"")]
    public TestColor Color { get; set; }

    [CommandParameter(2, Name = ""hue"")]
    public IReadOnlyList<TestColor> Hue { get; set;}

    [CommandOption(""opt"", 'o')]
    public string Option { get; set; } = """";

    [CommandOption(""opt01"", '1')]
    public string Option01 { get; set; } = """";

    [CommandOption(""opt02"", '2')]
    public string Option02 { get; set; } = """";

    public ValueTask ExecuteAsync(IConsole console) => default;
}
";
            var application = TestApplicationFactory(optCommandCs)
                .AllowSuggestMode()
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] { "[suggest]", "--envvar", "CLIFX-{GUID}", "--cursor", (variableContents.Length + cursorOffset).ToString() },
                new Dictionary<string, string>()
                {
                    ["CLIFX-{GUID}"] = variableContents
                }
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);

            stdOut.Split(null)
                  .Where(p => !string.IsNullOrWhiteSpace(p))
                  .Should().BeEquivalentTo(expected, usecase);
        }
    }
}