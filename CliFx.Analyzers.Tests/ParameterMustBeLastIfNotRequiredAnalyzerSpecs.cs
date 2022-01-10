using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests;

public class ParameterMustBeLastIfNotRequiredAnalyzerSpecs
{
    private static DiagnosticAnalyzer Analyzer { get; } = new ParameterMustBeLastIfNotRequiredAnalyzer();

    [Fact]
    public void Analyzer_reports_an_error_if_a_non_required_parameter_is_not_the_last_in_order()
    {
        // Arrange
        // language=cs
        const string code = @"
[Command]
public class MyCommand : ICommand
{
    [CommandParameter(0, Name = ""foo"", IsRequired = false)]
    public string Foo { get; set; }

    [CommandParameter(1, Name = ""bar"", IsRequired = false)]
    public string Bar { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}";

        // Act & assert
        Analyzer.Should().ProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_if_a_non_required_parameter_is_the_last_in_order()
    {
        // Arrange
        // language=cs
        const string code = @"
[Command]
public class MyCommand : ICommand
{
    [CommandParameter(0, Name = ""foo"", IsRequired = true)]
    public string Foo { get; set; }

    [CommandParameter(1, Name = ""bar"", IsRequired = false)]
    public string Bar { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}";

        // Act & assert
        Analyzer.Should().NotProduceDiagnostics(code);
    }

    [Fact]
    public void Analyzer_does_not_report_an_error_if_no_non_required_parameters_are_defined()
    {
        // Arrange
        // language=cs
        const string code = @"
[Command]
public class MyCommand : ICommand
{
    [CommandParameter(0, Name = ""foo"")]
    public string Foo { get; set; }

    [CommandParameter(1, Name = ""bar"", IsRequired = true)]
    public string Bar { get; set; }

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