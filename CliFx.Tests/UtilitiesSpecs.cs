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
            using var console = new VirtualConsole(false);
            var ticker = console.CreateProgressTicker();

            var progressValues = Enumerable.Range(0, 100).Select(p => p / 100.0).ToArray();
            var progressStringValues = progressValues.Select(p => p.ToString("P2")).ToArray();

            // Act
            foreach (var progress in progressValues)
                ticker.Report(progress);

            // Assert
            console.ReadOutputString().Should().ContainAll(progressStringValues);
        }

        [Fact]
        public void Progress_ticker_does_not_write_to_console_if_output_is_redirected()
        {
            // Arrange
            using var console = new VirtualConsole();
            var ticker = console.CreateProgressTicker();

            var progressValues = Enumerable.Range(0, 100).Select(p => p / 100.0).ToArray();

            // Act
            foreach (var progress in progressValues)
                ticker.Report(progress);

            // Assert
            console.ReadOutputString().Should().BeEmpty();
        }
    }
}