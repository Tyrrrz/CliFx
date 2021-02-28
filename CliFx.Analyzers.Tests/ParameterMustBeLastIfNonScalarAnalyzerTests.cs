using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests
{
    public class ParameterMustBeLastIfNonScalarAnalyzerTests
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new ParameterMustBeLastIfNonScalarAnalyzer();

        [Fact]
        public void Analyzer_reports_an_error_if_a_non_scalar_parameter_is_not_last_in_order()
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
    public string Bar { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().ProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_a_non_scalar_parameter_is_last_in_order()
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
    }
}