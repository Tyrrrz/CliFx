using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests;

public class OptionMustBeRequiredIfPropertyRequiredAnalyzerSpecs
{
    private static DiagnosticAnalyzer Analyzer { get; } = new OptionMustBeRequiredIfPropertyRequiredAnalyzer();

    [Fact]
    public void Analyzer_reports_an_error_if_a_non_required_option_is_bound_to_a_required_property()
    {
        // Arrange
        // language=cs
        const string code =
            """
            [Command]
            public class MyCommand : ICommand
            {
                [CommandOption('f', IsRequired = false)]
                public required string Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().ProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_if_a_required_option_is_bound_to_a_required_property()
    {
        // Arrange
        // language=cs
        const string code =
            """
            [Command]
            public class MyCommand : ICommand
            {
                [CommandOption('f')]
                public required string Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().NotProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_if_a_non_required_option_is_bound_to_a_non_required_property()
    {
        // Arrange
        // language=cs
        const string code =
            """
            [Command]
            public class MyCommand : ICommand
            {
                [CommandOption('f', IsRequired = false)]
                public string? Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().NotProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_if_a_required_option_is_bound_to_a_non_required_property()
    {
        // Arrange
        // language=cs
        const string code =
            """
            [Command]
            public class MyCommand : ICommand
            {
                [CommandOption('f')]
                public required string Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().NotProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_on_a_property_that_is_not_an_option()
    {
        // Arrange
        // language=cs
        const string code =
            """
            [Command]
            public class MyCommand : ICommand
            {
                public required string Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().NotProduceDiagnostics(code);
    }
}