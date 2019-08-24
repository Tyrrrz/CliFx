using System.Globalization;
using System.IO;
using System.Linq;
using CliFx.Services;
using CliFx.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests.Utilities
{
    [TestFixture]
    public class ProgressTickerTests
    {
        [Test]
        public void Report_Test()
        {
            // Arrange
            var formatProvider = CultureInfo.InvariantCulture;

            using (var stdout = new StringWriter(formatProvider))
            {
                var console = new VirtualConsole(TextReader.Null, false, stdout, false, TextWriter.Null, false);
                var ticker = console.CreateProgressTicker();

                var progressValues = Enumerable.Range(0, 100).Select(p => p / 100.0).ToArray();
                var progressStringValues = progressValues.Select(p => p.ToString("P2", formatProvider)).ToArray();

                // Act
                foreach (var progress in progressValues)
                    ticker.Report(progress);

                // Assert
                stdout.ToString().Should().ContainAll(progressStringValues);
            }
        }

        [Test]
        public void Report_Redirected_Test()
        {
            // Arrange
            using (var stdout = new StringWriter())
            {
                var console = new VirtualConsole(stdout);
                var ticker = console.CreateProgressTicker();

                var progressValues = Enumerable.Range(0, 100).Select(p => p / 100.0).ToArray();

                // Act
                foreach (var progress in progressValues)
                    ticker.Report(progress);

                // Assert
                stdout.ToString().Should().BeEmpty();
            }
        }
    }
}