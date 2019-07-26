using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Models;
using CliFx.Services;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests
{
    public partial class CommandSchemaResolverTests
    {
        [Command("Command name", Description = "Command description")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        private class TestCommand : ICommand
        {
            [CommandOption("option-a", 'a', GroupName = "Group 1")]
            public int OptionA { get; set; }

            [CommandOption("option-b", IsRequired = true)]
            public string OptionB { get; set; }

            [CommandOption("option-c", Description = "Option C description")]
            public bool OptionC { get; set; }

            public Task ExecuteAsync(CommandContext context) => throw new NotImplementedException();
        }
    }

    [TestFixture]
    public partial class CommandSchemaResolverTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_ResolveAllSchemas()
        {
            yield return new TestCaseData(
                typeof(TestCommand),
                new CommandSchema(typeof(TestCommand), "Command name", "Command description",
                    new[]
                    {
                        new CommandOptionSchema(typeof(TestCommand).GetProperty(nameof(TestCommand.OptionA)),
                            "option-a", 'a', "Group 1", false, null),
                        new CommandOptionSchema(typeof(TestCommand).GetProperty(nameof(TestCommand.OptionB)),
                            "option-b", null, null, true, null),
                        new CommandOptionSchema(typeof(TestCommand).GetProperty(nameof(TestCommand.OptionC)),
                            "option-c", null, null, false, "Option C description")
                    })
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_ResolveAllSchemas))]
        public void GetCommandSchema_Test(Type commandType, CommandSchema expectedSchema)
        {
            // Arrange
            var resolver = new CommandSchemaResolver();

            // Act
            var schema = resolver.GetCommandSchema(commandType);

            // Assert
            schema.Should().BeEquivalentTo(expectedSchema);
        }
    }
}