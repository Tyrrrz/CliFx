using CliFx.Tests.Commands;
using CliFx.Utilities;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class CommandLineUtilsSpecs
    {

        private readonly ITestOutputHelper _output;

        public CommandLineUtilsSpecs(ITestOutputHelper output) => _output = output;

        [Theory]
        [InlineData("MyApp alpha beta", new string[] { "MyApp", "alpha", "beta" })]
        [InlineData("MyApp \"alpha with spaces\" \"beta with spaces\"", new string[] { "MyApp","alpha with spaces", "beta with spaces" })]
        [InlineData("MyApp 'alpha with spaces' beta", new string[] { "MyApp", "'alpha", "with", "spaces'", "beta"})]
        [InlineData("MyApp \\\\\\alpha \\\\\\\\\"beta", new string[] { "MyApp", "\\\\\\alpha", "\\\\beta" })]
        [InlineData("MyApp \\\\\\\\\\\"alpha \\\"beta", new string[] { "MyApp", "\\\\\"alpha", "\"beta" })]
        public void Suggestion_service_can_emulate_GetCommandLineArgs(string input, string[] expected)
        {
            var output = CommandLineUtils.GetCommandLineArgsV(input);
            output.Should().BeEquivalentTo(expected);
        }
    }
}
