using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CliFx.Models;
using CliFx.Services;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests.Services
{
    [TestFixture]
    public class CommandArgumentSchemasValidatorTests
    {
        private static CommandArgumentSchema GetValidArgumentSchema(string propertyName, string name, bool isRequired, int order, string? description = null)
        {
            return new CommandArgumentSchema(typeof(TestCommand).GetProperty(propertyName)!, name, isRequired, description, order);
        }

        private static IEnumerable<TestCaseData> GetTestCases_ValidatorTest()
        {
            // Validation should succeed when no arguments are supplied
            yield return new TestCaseData(new ValidatorTest(new List<CommandArgumentSchema>(), true));
            
            // Multiple sequence arguments
            yield return new TestCaseData(new ValidatorTest(
                new []
                {
                    GetValidArgumentSchema(nameof(TestCommand.EnumerableProperty), "A", false, 0),
                    GetValidArgumentSchema(nameof(TestCommand.EnumerableProperty), "B", false, 1)
                }, false));
            
            // Argument after sequence
            yield return new TestCaseData(new ValidatorTest(
                new []
                {
                    GetValidArgumentSchema(nameof(TestCommand.EnumerableProperty), "A", false, 0),
                    GetValidArgumentSchema(nameof(TestCommand.StringProperty), "B", false, 1)
                }, false));
            yield return new TestCaseData(new ValidatorTest(
                new []
                {
                    GetValidArgumentSchema(nameof(TestCommand.StringProperty), "B", false, 0),
                    GetValidArgumentSchema(nameof(TestCommand.EnumerableProperty), "A", false, 1)
                }, true));
            
            // Required arguments must appear before optional arguments
            yield return new TestCaseData(new ValidatorTest(
                new []
                {
                    GetValidArgumentSchema(nameof(TestCommand.StringProperty), "A", false, 0),
                    GetValidArgumentSchema(nameof(TestCommand.StringProperty), "B", true, 1)
                }, false));
            yield return new TestCaseData(new ValidatorTest(
                new []
                {
                    GetValidArgumentSchema(nameof(TestCommand.StringProperty), "A", false, 0),
                    GetValidArgumentSchema(nameof(TestCommand.StringProperty), "B", true, 1),
                    GetValidArgumentSchema(nameof(TestCommand.StringProperty), "C", false, 2),
                }, false));
            yield return new TestCaseData(new ValidatorTest(
                new []
                {
                    GetValidArgumentSchema(nameof(TestCommand.StringProperty), "A", true, 0),
                    GetValidArgumentSchema(nameof(TestCommand.StringProperty), "B", false, 1),
                    GetValidArgumentSchema(nameof(TestCommand.StringProperty), "C", true, 2),
                }, false));
            yield return new TestCaseData(new ValidatorTest(
                new []
                {
                    GetValidArgumentSchema(nameof(TestCommand.StringProperty), "A", true, 0),
                    GetValidArgumentSchema(nameof(TestCommand.StringProperty), "B", false, 1),
                    GetValidArgumentSchema(nameof(TestCommand.StringProperty), "C", false, 2),
                }, true));
            
            // Argument order must be unique
            yield return new TestCaseData(new ValidatorTest(
                new []
                {
                    GetValidArgumentSchema(nameof(TestCommand.StringProperty), "A", false, 0),
                    GetValidArgumentSchema(nameof(TestCommand.StringProperty), "B", false, 1),
                    GetValidArgumentSchema(nameof(TestCommand.StringProperty), "C", false, 2)
                }, true));
            yield return new TestCaseData(new ValidatorTest(
                new []
                {
                    GetValidArgumentSchema(nameof(TestCommand.StringProperty), "A", false, 0),
                    GetValidArgumentSchema(nameof(TestCommand.StringProperty), "B", false, 1),
                    GetValidArgumentSchema(nameof(TestCommand.StringProperty), "C", false, 1)
                }, false));
            
            // No arguments with the same name
            yield return new TestCaseData(new ValidatorTest(
                new []
                {
                    GetValidArgumentSchema(nameof(TestCommand.StringProperty), "A", false, 0),
                    GetValidArgumentSchema(nameof(TestCommand.EnumerableProperty), "A", false, 1)
                }, false));
        }

        private class TestCommand
        {
            public IEnumerable<int> EnumerableProperty { get; set; }
            public string StringProperty { get; set; }
        }

        public class ValidatorTest
        {
            public ValidatorTest(IReadOnlyCollection<CommandArgumentSchema> schemas, bool succeedsValidation)
            {
                Schemas = schemas;
                SucceedsValidation = succeedsValidation;
            }
            
            public IReadOnlyCollection<CommandArgumentSchema> Schemas { get; }
            public bool SucceedsValidation { get; }
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_ValidatorTest))]
        public void Validation_Test(ValidatorTest testCase)
        {
            // Arrange
            var validator = new CommandArgumentSchemasValidator();
            
            // Act
            var result = validator.ValidateArgumentSchemas(testCase.Schemas);
            
            // Assert
            result.Any().Should().Be(!testCase.SucceedsValidation);
        }
    }
}