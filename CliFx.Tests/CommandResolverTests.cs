using System.Collections.Generic;
using CliFx.Exceptions;
using CliFx.Models;
using CliFx.Services;
using CliFx.Tests.TestObjects;
using Moq;
using NUnit.Framework;

namespace CliFx.Tests
{
    [TestFixture]
    public class CommandResolverTests
    {
        private static IEnumerable<TestCaseData> GetData_ResolveCommand()
        {
            yield return new TestCaseData(
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"int", "13"}
                }),
                new TestCommand {IntOption = 13}
            );

            yield return new TestCaseData(
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"int", "13"},
                    {"str", "hello world" }
                }),
                new TestCommand { IntOption = 13, StringOption = "hello world"}
            );

            yield return new TestCaseData(
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"i", "13"}
                }),
                new TestCommand { IntOption = 13 }
            );

            yield return new TestCaseData(
                new CommandOptionSet("command", new Dictionary<string, string>
                {
                    {"int", "13"}
                }),
                new TestCommand { IntOption = 13 }
            );
        }

        [Test]
        [TestCaseSource(nameof(GetData_ResolveCommand))]
        public void ResolveCommand_Test(CommandOptionSet commandOptionSet, TestCommand expectedCommand)
        {
            // Arrange
            var commandTypes = new[] {typeof(TestCommand)};

            var typeProviderMock = new Mock<ITypeProvider>();
            typeProviderMock.Setup(m => m.GetTypes()).Returns(commandTypes);
            var typeProvider = typeProviderMock.Object;

            var optionParserMock = new Mock<ICommandOptionParser>();
            optionParserMock.Setup(m => m.ParseOptions(It.IsAny<IReadOnlyList<string>>())).Returns(commandOptionSet);
            var optionParser = optionParserMock.Object;

            var optionConverter = new CommandOptionConverter();

            var resolver = new CommandResolver(typeProvider, optionParser, optionConverter);

            // Act
            var command = resolver.ResolveCommand() as TestCommand;

            // Assert
            Assert.That(command, Is.Not.Null);
            Assert.That(command.StringOption, Is.EqualTo(expectedCommand.StringOption), nameof(command.StringOption));
            Assert.That(command.IntOption, Is.EqualTo(expectedCommand.IntOption), nameof(command.IntOption));
        }

        private static IEnumerable<TestCaseData> GetData_ResolveCommand_IsRequired()
        {
            yield return new TestCaseData(CommandOptionSet.Empty);

            yield return new TestCaseData(
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"str", "hello world"}
                })
            );
        }

        [Test]
        [TestCaseSource(nameof(GetData_ResolveCommand_IsRequired))]
        public void ResolveCommand_IsRequired_Test(CommandOptionSet commandOptionSet)
        {
            // Arrange
            var commandTypes = new[] { typeof(TestCommand) };

            var typeProviderMock = new Mock<ITypeProvider>();
            typeProviderMock.Setup(m => m.GetTypes()).Returns(commandTypes);
            var typeProvider = typeProviderMock.Object;

            var optionParserMock = new Mock<ICommandOptionParser>();
            optionParserMock.Setup(m => m.ParseOptions(It.IsAny<IReadOnlyList<string>>())).Returns(commandOptionSet);
            var optionParser = optionParserMock.Object;

            var optionConverter = new CommandOptionConverter();

            var resolver = new CommandResolver(typeProvider, optionParser, optionConverter);

            // Act & Assert
            Assert.Throws<CommandResolveException>(() => resolver.ResolveCommand());
        }
    }
}