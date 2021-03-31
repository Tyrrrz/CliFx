using CliFx.Infrastructure;
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
    public class SuggestHookInstallerSpecs : SpecsBase
    {
        private FakeFileSystem FakeFileSystem => new ();

        public SuggestHookInstallerSpecs(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        public CliApplicationBuilder TestApplicationFactory(params string[] commandClasses)
        {
            var builder = new CliApplicationBuilder();
            return builder.UseConsole(FakeConsole);
        }

        [Theory]
        [InlineData("/usr/bin/bash", ".bashrc")]
        [InlineData("/usr/bin/pwsh", "Microsoft.PowerShell_profile.ps1")]
        [InlineData(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe", "Microsoft.PowerShell_profile.ps1")]
        public async Task Suggest_hook_is_added_when_suggestions_are_allowed(string shellPath, string expectedHookScript)
        {
            // Arrange
            var fileSystem = new FakeFileSystem();
            fileSystem.Files[shellPath] = "stub shell interpeter";

            var application = TestApplicationFactory()
                .AllowSuggestMode(true)
                .UseFileSystem(fileSystem)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new string[] { }
            );

            // Assert
            exitCode.Should().Be(0);
            fileSystem.FilePaths.Count(p => p.Contains(expectedHookScript))
                      .Should().Be(1, "expect to see hook script");

            var hookScript = fileSystem.Files.Where(p => p.Key.Contains(expectedHookScript)).First().Value;

            hookScript.Should().Contain($"### clifx-suggest-begins-here-{application.Metadata.Title}");
            hookScript.Should().Contain($"### clifx-suggest-ends-here-{application.Metadata.Title}");

        }

        [Fact]
        public async Task Suggest_hook_is_not_installed_when_suggestions_arent_allowed()
        {
            // Arrange
            var fileSystem = new FakeFileSystem();
            fileSystem.Files["/usr/bin/bash"] = "stub shell interpeter";

            var application = TestApplicationFactory()
                .AllowSuggestMode(false)
                .UseFileSystem(fileSystem)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new string[] {  }
            );

            // Assert
            exitCode.Should().Be(0);
            fileSystem.FilePaths.Count(p => p.Contains(".bashrc")).Should().Be(0);
        }
    }
}
