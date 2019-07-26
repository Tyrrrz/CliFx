using System;
using System.Collections;
using System.Collections.Generic;
using CliFx.Models;
using CliFx.Services;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests
{
    public partial class CommandOptionInputConverterTests
    {
        public enum TestEnum
        {
            Value1,
            Value2,
            Value3
        }

        public struct TestStringConstructable
        {
            public string Value { get; }

            public TestStringConstructable(string value)
            {
                Value = value;
            }
        }

        public struct TestStringParseable
        {
            public string Value { get; }

            private TestStringParseable(string value)
            {
                Value = value;
            }

            public static TestStringParseable Parse(string value) => new TestStringParseable(value);
        }
    }

    [TestFixture]
    public partial class CommandOptionInputConverterTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_ConvertOption()
        {
            yield return new TestCaseData(
                new CommandOptionInput("option", "value"),
                typeof(string),
                "value"
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "value"),
                typeof(object),
                "value"
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "true"),
                typeof(bool),
                true
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "false"),
                typeof(bool),
                false
            );

            yield return new TestCaseData(
                new CommandOptionInput("option"),
                typeof(bool),
                true
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "123"),
                typeof(int),
                123
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "123.45"),
                typeof(double),
                123.45
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "28 Apr 1995"),
                typeof(DateTime),
                new DateTime(1995, 04, 28)
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "28 Apr 1995"),
                typeof(DateTimeOffset),
                new DateTimeOffset(new DateTime(1995, 04, 28))
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "00:14:59"),
                typeof(TimeSpan),
                new TimeSpan(00, 14, 59)
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "value2"),
                typeof(TestEnum),
                TestEnum.Value2
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "666"),
                typeof(int?),
                666
            );

            yield return new TestCaseData(
                new CommandOptionInput("option"),
                typeof(int?),
                null
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "value3"),
                typeof(TestEnum?),
                TestEnum.Value3
            );

            yield return new TestCaseData(
                new CommandOptionInput("option"),
                typeof(TestEnum?),
                null
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "01:00:00"),
                typeof(TimeSpan?),
                new TimeSpan(01, 00, 00)
            );

            yield return new TestCaseData(
                new CommandOptionInput("option"),
                typeof(TimeSpan?),
                null
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "value"),
                typeof(TestStringConstructable),
                new TestStringConstructable("value")
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "value"),
                typeof(TestStringParseable),
                TestStringParseable.Parse("value")
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", new[] {"value1", "value2"}),
                typeof(string[]),
                new[] {"value1", "value2"}
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", new[] {"value1", "value2"}),
                typeof(object[]),
                new[] {"value1", "value2"}
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", new[] {"47", "69"}),
                typeof(int[]),
                new[] {47, 69}
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", new[] {"value1", "value3"}),
                typeof(TestEnum[]),
                new[] {TestEnum.Value1, TestEnum.Value3}
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", new[] {"value1", "value2"}),
                typeof(IEnumerable),
                new[] {"value1", "value2"}
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", new[] {"value1", "value2"}),
                typeof(IEnumerable<string>),
                new[] {"value1", "value2"}
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", new[] {"value1", "value2"}),
                typeof(IReadOnlyList<string>),
                new[] {"value1", "value2"}
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_ConvertOption))]
        public void ConvertOption_Test(CommandOptionInput optionInput, Type targetType, object expectedConvertedValue)
        {
            // Arrange
            var converter = new CommandOptionInputConverter();

            // Act
            var convertedValue = converter.ConvertOption(optionInput, targetType);

            // Assert
            convertedValue.Should().BeEquivalentTo(expectedConvertedValue);

            if (convertedValue != null)
                convertedValue.Should().BeAssignableTo(targetType);
        }
    }
}