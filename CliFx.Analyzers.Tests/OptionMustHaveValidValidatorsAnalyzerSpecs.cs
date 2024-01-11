using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests;

public class OptionMustHaveValidValidatorsAnalyzerSpecs
{
    private static DiagnosticAnalyzer Analyzer { get; } =
        new OptionMustHaveValidValidatorsAnalyzer();

    [Fact]
    public void Analyzer_reports_an_error_if_an_option_has_a_validator_that_does_not_derive_from_BindingValidator()
    {
        // Arrange
        // lang=csharp
        const string code = """
            public class MyValidator
            {
                public void Validate(string value) {}
            }
            
            [Command]
            public class MyCommand : ICommand
            {
                [CommandOption("foo", Validators = new[] { typeof(MyValidator) })]
                public string? Foo { get; init; }
            
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().ProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_reports_an_error_if_an_option_has_a_validator_that_does_not_derive_from_a_compatible_BindingValidator()
    {
        // Arrange
        // lang=csharp
        const string code = """
            public class MyValidator : BindingValidator<int>
            {
                public override BindingValidationError Validate(int value) => Ok();
            }
            
            [Command]
            public class MyCommand : ICommand
            {
                [CommandOption("foo", Validators = new[] { typeof(MyValidator) })]
                public string? Foo { get; init; }
            
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().ProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_if_an_option_has_validators_that_all_derive_from_compatible_BindingValidators()
    {
        // Arrange
        // lang=csharp
        const string code = """
            public class MyValidator : BindingValidator<string>
            {
                public override BindingValidationError Validate(string value) => Ok();
            }
            
            [Command]
            public class MyCommand : ICommand
            {
                [CommandOption("foo", Validators = new[] { typeof(MyValidator) })]
                public string? Foo { get; init; }
            
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """;

        // Act & assert
        Analyzer.Should().NotProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_if_an_option_does_not_have_validators()
    {
        // Arrange
        // lang=csharp
        const string code = """
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
    public void Analyzer_does_not_report_an_error_on_a_property_that_is_not_an_option()
    {
        // Arrange
        // lang=csharp
        const string code = """
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
