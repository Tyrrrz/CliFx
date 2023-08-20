using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests;

public class OptionMustBeInsideCommandAnalyzerSpecs
{
    private static DiagnosticAnalyzer Analyzer { get; } = new OptionMustBeInsideCommandAnalyzer();

    [Fact]
    public void Analyzer_reports_an_error_if_an_option_is_inside_a_class_that_is_not_a_command()
    {
        // Arrange
        // lang=csharp
        const string code =
            """
            public class MyClass
            {
                [CommandOption("foo")]
                public string? Foo { get; init; }
            }
            """;

        // Act & assert
        Analyzer.Should().ProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_if_an_option_is_inside_a_command()
    {
        // Arrange
        // lang=csharp
        const string code =
            """
            [Command]
            public class MyCommand : ICommand
            {
                [CommandOption("foo")]
                public string? Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().NotProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_if_an_option_is_inside_an_abstract_class()
    {
        // Arrange
        // lang=csharp
        const string code =
            """
            public abstract class MyCommand
            {
                [CommandOption("foo")]
                public string? Foo { get; init; }
            }
            """;

        // Act & assert
        Analyzer.Should().NotProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_on_a_property_that_is_not_an_option()
    {
        // Arrange
        // lang=csharp
        const string code =
            """
            [Command]
            public class MyCommand : ICommand
            {
                public string? Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().NotProduceDiagnostics(code);
    }
}