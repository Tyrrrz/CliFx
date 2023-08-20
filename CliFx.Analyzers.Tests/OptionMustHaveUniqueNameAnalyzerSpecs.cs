using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests;

public class OptionMustHaveUniqueNameAnalyzerSpecs
{
    private static DiagnosticAnalyzer Analyzer { get; } = new OptionMustHaveUniqueNameAnalyzer();

    [Fact]
    public void Analyzer_reports_an_error_if_an_option_has_the_same_name_as_another_option()
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
            
                [CommandOption("foo")]
                public string? Bar { get; init; }
            
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().ProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_if_an_option_has_a_unique_name()
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
            
                [CommandOption("bar")]
                public string? Bar { get; init; }
            
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().NotProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_if_an_option_does_not_have_a_name()
    {
        // Arrange
        // lang=csharp
        const string code =
            """
            [Command]
            public class MyCommand : ICommand
            {
                [CommandOption('f')]
                public string? Foo { get; init; }
            
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