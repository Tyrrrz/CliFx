using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests
{
    public class SystemConsoleShouldBeAvoidedAnalyzerSpecs
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new SystemConsoleShouldBeAvoidedAnalyzer();

        [Fact]
        public void Analyzer_reports_an_error_if_a_command_calls_a_method_on_SystemConsole()
        {
            // Arrange
            // language=cs
            const string code = @"
[Command]
public class MyCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        Console.WriteLine(""Hello world"");
        return default;
    }
}";

            // Act & assert
            Analyzer.Should().ProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_reports_an_error_if_a_command_accesses_a_property_on_SystemConsole()
        {
            // Arrange
            // language=cs
            const string code = @"
[Command]
public class MyCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        Console.ForegroundColor = ConsoleColor.Black;
        return default;
    }
}";
            // Act & assert
            Analyzer.Should().ProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_reports_an_error_if_a_command_calls_a_method_on_a_property_of_SystemConsole()
        {
            // Arrange
            // language=cs
            const string code = @"
[Command]
public class MyCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        Console.Error.WriteLine(""Hello world"");
        return default;
    }
}";

            // Act & assert
            Analyzer.Should().ProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_a_command_interacts_with_the_console_through_IConsole()
        {
            // Arrange
            // language=cs
            const string code = @"
[Command]
public class MyCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""Hello world"");
        return default;
    }
}";

            // Act & assert
            Analyzer.Should().NotProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_IConsole_is_not_available_in_the_current_method()
        {
            // Arrange
            // language=cs
            const string code = @"
[Command]
public class MyCommand : ICommand
{
    public void SomeOtherMethod() => Console.WriteLine(""Test"");

    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().NotProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_a_command_does_not_access_SystemConsole()
        {
            // Arrange
            // language=cs
            const string code = @"
[Command]
public class MyCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        return default;
    }
}";

            // Act & assert
            Analyzer.Should().NotProduceDiagnostics(code);
        }
    }
}