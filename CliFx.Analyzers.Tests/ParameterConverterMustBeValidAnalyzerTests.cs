using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests
{
    public class ParameterConverterMustBeValidAnalyzerTests
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new ParameterConverterMustBeValidAnalyzer();

        [Fact]
        public void Analyzer_reports_an_error_if_the_specified_parameter_converter_does_not_implement_IArgumentValueConverter()
        {
            // Arrange
            // language=cs
            const string code = @"
public class MyConverter
{
    public object ConvertFrom(string value) => value;
}

[Command]
public class MyCommand : ICommand
{
    [CommandParameter(0, Converter = typeof(MyConverter))]
    public string Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().ProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_the_specified_parameter_converter_implements_IArgumentValueConverter()
        {
            // Arrange
            // language=cs
            const string code = @"
public class MyConverter : IArgumentValueConverter
{
    public object ConvertFrom(string value) => value;
}

[Command]
public class MyCommand : ICommand
{
    [CommandParameter(0, Converter = typeof(MyConverter))]
    public string Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().NotProduceDiagnostics(code);
        }
    }
}