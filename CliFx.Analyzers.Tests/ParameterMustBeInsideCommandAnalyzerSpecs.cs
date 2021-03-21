using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests
{
    public class ParameterMustBeInsideCommandAnalyzerSpecs
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new ParameterMustBeInsideCommandAnalyzer();

        [Fact]
        public void Analyzer_reports_an_error_if_a_parameter_is_inside_a_class_that_is_not_a_command()
        {
            // Arrange
            // language=cs
            const string code = @"
public class MyClass
{
    [CommandParameter(0)]
    public string Foo { get; set; }
}";

            // Act & assert
            Analyzer.Should().ProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_a_parameter_is_inside_a_command()
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
        public void Analyzer_does_not_report_an_error_if_a_parameter_is_inside_an_abstract_class()
        {
            // Arrange
            // language=cs
            const string code = @"
public abstract class MyCommand
{
    [CommandParameter(0)]
    public string Foo { get; set; }
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