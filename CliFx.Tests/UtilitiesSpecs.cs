using System.IO;
using System.Linq;
using CliFx.Utilities;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests
{
    public class UtilitiesSpecs
    {
        [Fact]
        public void Progress_ticker_can_be_used_to_report_progress_to_console()
        {
            // Arrange
            using var stdOut = new MemoryStream();
            var console = new VirtualConsole(output: stdOut, isOutputRedirected: false);

            var ticker = console.CreateProgressTicker();

            var progressValues = Enumerable.Range(0, 100).Select(p => p / 100.0).ToArray();
            var progressStringValues = progressValues.Select(p => p.ToString("P2")).ToArray();

            // Act
            foreach (var progress in progressValues)
                ticker.Report(progress);

            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray());

            // Assert
            stdOutData.Should().ContainAll(progressStringValues);
        }

        [Fact]
        public void Progress_ticker_does_not_write_to_console_if_output_is_redirected()
        {
            // Arrange
            using var stdOut = new MemoryStream();
            var console = new VirtualConsole(output: stdOut);

            var ticker = console.CreateProgressTicker();

            var progressValues = Enumerable.Range(0, 100).Select(p => p / 100.0).ToArray();

            // Act
            foreach (var progress in progressValues)
                ticker.Report(progress);

            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray());

            // Assert
            stdOutData.Should().BeEmpty();
        }
    }
}