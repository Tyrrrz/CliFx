using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests
{
    public class ParameterMustHaveValidValidatorsAnalyzerSpecs
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new ParameterMustHaveValidValidatorsAnalyzer();

        [Fact]
        public void Analyzer_reports_an_error_if_one_of_the_specified_parameter_validators_does_not_derive_from_BindingValidator()
        {
            // Arrange
            // language=cs
            const string code = @"
public class MyValidator
{
    public void Validate(string value) {}
}

[Command]
public class MyCommand : ICommand
{
    [CommandParameter(0, Validators = new[] {typeof(MyValidator)})]
    public string Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().ProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_all_specified_parameter_validators_derive_from_BindingValidator()
        {
            // Arrange
            // language=cs
            const string code = @"
public class MyValidator : BindingValidator<string>
{
    public override BindingValidationError Validate(string value) => Ok();
}

[Command]
public class MyCommand : ICommand
{
    [CommandParameter(0, Validators = new[] {typeof(MyValidator)})]
    public string Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().NotProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_a_parameter_does_not_have_validators()
        {
            // Arrange
            // language=cs
            const string code = @"
[Command]
public class MyCommand : ICommand
{
    [CommandParameter(0)]
    public string Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().NotProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_on_a_property_that_is_not_a_parameter()
        {
            // Arrange
            // language=cs
            const string code = @"
[Command]
public class MyCommand : ICommand
{
    public string Foo { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().NotProduceDiagnostics(code);
        }
    }
}