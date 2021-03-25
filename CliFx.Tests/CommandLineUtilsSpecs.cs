using CliFx.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class CommandLineSplitterSpecs 
    {
        [Theory]
        [InlineData("MyApp alpha beta", new string[] { "MyApp", "alpha", "beta" })]
        [InlineData("MyApp \"alpha with spaces\" \"beta with spaces\"", new string[] { "MyApp", "alpha with spaces", "beta with spaces" })]
        [InlineData("MyApp 'alpha with spaces' beta", new string[] { "MyApp", "'alpha", "with", "spaces'", "beta" })]
        [InlineData("MyApp \\\\\\alpha \\\\\\\\\"beta", new string[] { "MyApp", "\\\\\\alpha", "\\\\beta" })]
        [InlineData("MyApp \\\\\\\\\\\"alpha \\\"beta", new string[] { "MyApp", "\\\\\"alpha", "\"beta" })]
        public void Suggestion_service_can_emulate_GetCommandLineArgs(string input, string[] expected)
        {
            var output = CommandLineSplitter.Split(input);
            output.Should().BeEquivalentTo(expected);
        }
    }
}
