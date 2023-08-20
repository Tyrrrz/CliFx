using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests;

public class ParameterMustHaveUniqueNameAnalyzerSpecs
{
    private static DiagnosticAnalyzer Analyzer { get; } = new ParameterMustHaveUniqueNameAnalyzer();

    [Fact]
    public void Analyzer_reports_an_error_if_a_parameter_has_the_same_name_as_another_parameter()
    {
        // Arrange
        // lang=csharp
        const string code =
            """
            [Command]
            public class MyCommand : ICommand
            {
                [CommandParameter(0, Name = "foo")]
                public required string Foo { get; init; }

                [CommandParameter(1, Name = "foo")]
                public required string Bar { get; init; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().ProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_if_a_parameter_has_unique_name()
    {
        // Arrange
        // lang=csharp
        const string code =
            """
            [Command]
            public class MyCommand : ICommand
            {
                [CommandParameter(0, Name = "foo")]
                public required string Foo { get; init; }

                [CommandParameter(1, Name = "bar")]
                public required string Bar { get; init; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().NotProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_on_a_property_that_is_not_a_parameter()
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