using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests
{
    public class ArgumentMustHaveValidConverterAnalyzerSpecs
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new ArgumentMustHaveValidConverterAnalyzer();

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
        public void Analyzer_reports_an_error_if_the_specified_option_converter_does_not_implement_IArgumentValueConverter()
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
    [CommandOption(""foo"", Converter = typeof(MyConverter))]
    public string Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().ProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_the_specified_option_converter_implements_IArgumentValueConverter()
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
    [CommandOption(""foo"", Converter = typeof(MyConverter))]
    public string Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().NotProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_an_option_does_not_have_a_converter()
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
        public void Analyzer_does_not_report_an_error_on_a_property_which_is_not_an_argument()
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