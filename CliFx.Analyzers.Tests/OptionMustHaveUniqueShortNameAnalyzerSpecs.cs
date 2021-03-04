using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests
{
    public class OptionMustHaveUniqueShortNameAnalyzerSpecs
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new OptionMustHaveUniqueShortNameAnalyzer();

        [Fact]
        public void Analyzer_reports_an_error_if_an_option_has_the_same_short_name_as_another_option()
        {
            // Arrange
            // language=cs
            const string code = @"
[Command]
public class MyCommand : ICommand
{
    [CommandOption('f')]
    public string Foo { get; set; }
    
    [CommandOption('f')]
    public string Bar { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().ProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_an_option_has_a_unique_short_name()
        {
            // Arrange
            // language=cs
            const string code = @"
[Command]
public class MyCommand : ICommand
{
    [CommandOption('f')]
    public string Foo { get; set; }
    
    [CommandOption('b')]
    public string Bar { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().NotProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_an_option_has_a_short_name_which_is_unique_only_in_casing()
        {
            // Arrange
            // language=cs
            const string code = @"
[Command]
public class MyCommand : ICommand
{
    [CommandOption('f')]
    public string Foo { get; set; }
    
    [CommandOption('F')]
    public string Bar { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().NotProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_an_option_does_not_have_a_short_name()
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