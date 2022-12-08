using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests;

public class CommandMustImplementInterfaceAnalyzerSpecs
{
    private static DiagnosticAnalyzer Analyzer { get; } = new CommandMustImplementInterfaceAnalyzer();

    [Fact]
    public void Analyzer_reports_an_error_if_a_command_does_not_implement_ICommand_interface()
    {
        // Arrange
        // language=cs
        const string code =
            """
            [Command]
            public class MyCommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().ProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_if_a_command_implements_ICommand_interface()
    {
        // Arrange
        // language=cs
        const string code =
            """
            [Command]
            public class MyCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().NotProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_on_a_class_that_is_not_a_command()
    {
        // Arrange
        // language=cs
        const string code =
            """
            public class Foo
            {
                public int Bar { get; set; } = 5;
            }
            """;

        // Act & assert
        Analyzer.Should().NotProduceDiagnostics(code);
    }
}