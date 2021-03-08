using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests
{
    public class ArgumentMustHaveValidValidatorsAnalyzerSpecs
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new ArgumentMustHaveValidValidatorsAnalyzer();

        [Fact]
        public void Analyzer_reports_an_error_if_one_of_the_specified_parameter_validators_does_not_derive_from_ArgumentValueValidator()
        {
            // Arrange
            // language=cs
            const string code = @"
public class MyValidator
{
    public ValidationResult Validate(string value) => ValidationResult.Ok();
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
        public void Analyzer_does_not_report_an_error_if_all_specified_parameter_validators_derive_from_ArgumentValueValidator()
        {
            // Arrange
            // language=cs
            const string code = @"
public class MyValidator : ArgumentValueValidator<string>
{
    public override ValidationResult Validate(string value) => ValidationResult.Ok();
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
        public void Analyzer_reports_an_error_if_one_of_the_specified_option_validators_does_not_derive_from_ArgumentValueValidator()
        {
            // Arrange
            // language=cs
            const string code = @"
public class MyValidator
{
    public ValidationResult Validate(string value) => ValidationResult.Ok();
}

[Command]
public class MyCommand : ICommand
{
    [CommandOption(""foo"", Validators = new[] {typeof(MyValidator)})]
    public string Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().ProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_all_specified_option_validators_derive_from_ArgumentValueValidator()
        {
            // Arrange
            // language=cs
            const string code = @"
public class MyValidator : ArgumentValueValidator<string>
{
    public override ValidationResult Validate(string value) => ValidationResult.Ok();
}

[Command]
public class MyCommand : ICommand
{
    [CommandOption(""foo"", Validators = new[] {typeof(MyValidator)})]
    public string Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().NotProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_an_option_does_not_have_validators()
        {
            // Arrange
            // language=cs
            const string code = @"
[Command]
public class MyCommand : ICommand
{
    [CommandOption(""foo"")]
    public string Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().NotProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_on_a_property_which_is_not_an_argument()
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