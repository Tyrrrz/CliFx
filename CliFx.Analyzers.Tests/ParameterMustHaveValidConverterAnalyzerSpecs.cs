using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests;

public class ParameterMustHaveValidConverterAnalyzerSpecs
{
    private static DiagnosticAnalyzer Analyzer { get; } = new ParameterMustHaveValidConverterAnalyzer();

    [Fact]
    public void Analyzer_reports_an_error_if_the_specified_parameter_converter_does_not_derive_from_BindingConverter()
    {
        // Arrange
        // language=cs
        const string code = @"
public class MyConverter
{
    public string Convert(string rawValue) => rawValue;
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
    public void Analyzer_does_not_report_an_error_if_the_specified_parameter_converter_derives_from_BindingConverter()
    {
        // Arrange
        // language=cs
        const string code = @"
public class MyConverter : BindingConverter<string>
{
    public override string Convert(string rawValue) => rawValue;
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

    [Fact]
    public void Analyzer_does_not_report_an_error_if_a_parameter_does_not_have_a_converter()
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