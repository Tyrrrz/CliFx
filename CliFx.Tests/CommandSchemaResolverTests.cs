using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Models;
using CliFx.Services;
using NUnit.Framework;

namespace CliFx.Tests
{
    public partial class CommandSchemaResolverTests
    {
        [Command(Description = "Command description")]
        public class TestCommand : ICommand
        {
            [CommandOption("option-a", 'a', GroupName = "Group 1")]
            public int OptionA { get; set; }

            [CommandOption("option-b", IsRequired = true)]
            public string OptionB { get; set; }

            [CommandOption("option-c", Description = "Option C description")]
            public bool OptionC { get; set; }

            public CommandContext Context { get; set; }

            public Task<ExitCode> ExecuteAsync() => throw new NotImplementedException();
        }
    }

    [TestFixture]
    public partial class CommandSchemaResolverTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_ResolveAllSchemas()
        {
            yield return new TestCaseData(
                new[] {typeof(TestCommand)},
                new[]
                {
                    new CommandSchema(typeof(TestCommand),
                        null, true, "Command description",
                        new[]
                        {
                            new CommandOptionSchema(typeof(TestCommand).GetProperty(nameof(TestCommand.OptionA)),
                                "option-a", 'a', false, "Group 1", null),
                            new CommandOptionSchema(typeof(TestCommand).GetProperty(nameof(TestCommand.OptionB)),
                                "option-b", null, true, null, null),
                            new CommandOptionSchema(typeof(TestCommand).GetProperty(nameof(TestCommand.OptionC)),
                                "option-c", null, false, null, "Option C description")
                        })
                }
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_ResolveAllSchemas))]
        public void ResolveAllSchemas_Test(IReadOnlyList<Type> sourceTypes, IReadOnlyList<CommandSchema> expectedSchemas)
        {
            // Arrange
            var resolver = new CommandSchemaResolver(sourceTypes);

            // Act
            var schemas = resolver.ResolveAllSchemas();

            // Assert
            Assert.That(schemas, Is.EqualTo(expectedSchemas).Using(CommandSchemaEqualityComparer.Instance));
        }
    }
}