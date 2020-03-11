using System.Linq;
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

        [Test]
        public void Report_Redirected_Test()
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