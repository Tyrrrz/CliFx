using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests
{
    public class ParameterMustBeSoleIfNonScalarAnalyzerTests
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new ParameterMustBeSoleIfNonScalarAnalyzer();

        [Fact]
        public void Analyzer_reports_an_error_if_more_than_one_non_scalar_parameters_are_defined()
        {
            // Arrange
            // language=cs
            const string code = @"
[Command]
public class MyCommand : ICommand
{
    [CommandParameter(0)]
    public string[] Foo { get; set; }
    
    [CommandParameter(1)]
    public string[] Bar { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().ProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_only_one_non_scalar_parameter_is_defined()
        {
            // Arrange
            // language=cs
            const string code = @"
[Command]
public class MyCommand : ICommand
{
    [CommandParameter(0)]
    public string Foo { get; set; }
    
    [CommandParameter(1)]
    public string[] Bar { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().NotProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_no_non_scalar_parameters_are_defined()
        {
            // Arrange
            // language=cs
            const string code = @"
[Command]
public class MyCommand : ICommand
{
    [CommandParameter(0)]
    public string Foo { get; set; }
    
    [CommandParameter(1)]
    public string Bar { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().NotProduceDiagnostics(code);
        }
    }
}