using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests
{
    public class OptionMustHaveUniqueEnvironmentVariableNameAnalyzerSpecs
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new OptionMustHaveUniqueEnvironmentVariableNameAnalyzer();

        [Fact]
        public void Analyzer_reports_an_error_if_an_option_has_the_same_environment_variable_name_as_another_option()
        {
            // Arrange
            // language=cs
            const string code = @"
[Command]
public class MyCommand : ICommand
{
    [CommandOption(""foo"", EnvironmentVariableName = ""env1"")]
    public string Foo { get; set; }
    
    [CommandOption(""bar"", EnvironmentVariableName = ""env1"")]
    public string Bar { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().ProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_an_option_has_unique_environment_variable_name()
        {
            // Arrange
            // language=cs
            const string code = @"
[Command]
public class MyCommand : ICommand
{
    [CommandOption(""foo"", EnvironmentVariableName = ""env1"")]
    public string Foo { get; set; }
    
    [CommandOption(""bar"", EnvironmentVariableName = ""env2"")]
    public string Bar { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().NotProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_an_option_does_not_have_an_environment_variable_name()
        {
            // Arrange
            // language=cs
            const string code = @"
[Command]
public class MyCommand : ICommand
{
    [CommandOption(""foo"")]
    public string Foo { get; set; }
    
    [CommandOption(""bar"")]
    public string Bar { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().NotProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_on_a_property_that_is_not_an_option()
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