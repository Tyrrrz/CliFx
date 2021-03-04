using System.Collections.Immutable;
using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests
{
    public class CommandMustBeAnnotatedAnalyzerSpecs
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new CommandMustBeAnnotatedAnalyzer();

        [Fact]
        public void Analyzer_reports_an_error_if_a_command_is_not_annotated_with_the_command_attribute()
        {
            // Arrange
            // language=cs
            const string code = @"
public class MyCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().ProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_a_command_is_implemented_as_an_abstract_class()
        {
            // Arrange
            // language=cs
            const string code = @"
public abstract class MyCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().NotProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_on_a_class_that_is_not_a_command()
        {
            // Arrange
            // language=cs
            const string code = @"
public class Foo
{
    public int Bar { get; set; } = 5;
}";

            // Act & assert
            Analyzer.Should().NotProduceDiagnostics(code);
        }
    }
}