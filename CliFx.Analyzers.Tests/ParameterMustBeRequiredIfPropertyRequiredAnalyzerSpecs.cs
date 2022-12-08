using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests;

public class ParameterMustBeRequiredIfPropertyRequiredAnalyzerSpecs
{
    private static DiagnosticAnalyzer Analyzer { get; } = new ParameterMustBeRequiredIfPropertyRequiredAnalyzer();

    [Fact]
    public void Analyzer_reports_an_error_if_a_non_required_parameter_is_bound_to_a_required_property()
    {
        // Arrange
        // language=cs
        const string code =
            """
            [Command]
            public class MyCommand : ICommand
            {
                [CommandParameter(0, IsRequired = false)]
                public required string Foo { get; set; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().ProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_if_a_required_parameter_is_bound_to_a_required_property()
    {
        // Arrange
        // language=cs
        const string code =
            """
            [Command]
            public class MyCommand : ICommand
            {
                [CommandParameter(0, IsRequired = true)]
                public required string Foo { get; set; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().NotProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_if_a_non_required_parameter_is_bound_to_an_unannotated_property()
    {
        // Arrange
        // language=cs
        const string code =
            """
            [Command]
            public class MyCommand : ICommand
            {
                [CommandParameter(0, IsRequired = false)]
                public string Foo { get; set; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().NotProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_if_a_required_parameter_is_bound_to_an_unannotated_property()
    {
        // Arrange
        // language=cs
        const string code =
            """
            [Command]
            public class MyCommand : ICommand
            {
                [CommandParameter(0, IsRequired = true)]
                public string Foo { get; set; }

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
        // language=cs
        const string code =
            """
            [Command]
            public class MyCommand : ICommand
            {
                public required string Foo { get; set; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().NotProduceDiagnostics(code);
    }
}