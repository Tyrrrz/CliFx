using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests;

public class ParameterMustBeLastIfNonScalarAnalyzerSpecs
{
    private static DiagnosticAnalyzer Analyzer { get; } = new ParameterMustBeLastIfNonScalarAnalyzer();

    [Fact]
    public void Analyzer_reports_an_error_if_a_non_scalar_parameter_is_not_the_last_in_order()
    {
        // Arrange
        // lang=csharp
        const string code =
            """
            [Command]
            public class MyCommand : ICommand
            {
                [CommandParameter(0)]
                public required string[] Foo { get; init; }

                [CommandParameter(1)]
                public required string Bar { get; init; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().ProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_if_a_non_scalar_parameter_is_the_last_in_order()
    {
        // Arrange
        // lang=csharp
        const string code =
            """
            [Command]
            public class MyCommand : ICommand
            {
                [CommandParameter(0)]
                public required string Foo { get; init; }

                [CommandParameter(1)]
                public required string[] Bar { get; init; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().NotProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_if_no_non_scalar_parameters_are_defined()
    {
        // Arrange
        // lang=csharp
        const string code =
            """
            [Command]
            public class MyCommand : ICommand
            {
                [CommandParameter(0)]
                public required string Foo { get; init; }

                [CommandParameter(1)]
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