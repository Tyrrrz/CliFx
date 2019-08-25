using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using CliFx.Exceptions;
using CliFx.Models;
using CliFx.Services;
using CliFx.Tests.TestCustomTypes;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests.Services
{
    [TestFixture]
    public class CommandOptionInputConverterTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_ConvertOptionInput()
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
                new CommandOptionInput("option", "a"),
                typeof(char),
                'a'
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "15"),
                typeof(sbyte),
                (sbyte) 15
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "15"),
                typeof(byte),
                (byte) 15
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "15"),
                typeof(short),
                (short) 15
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "15"),
                typeof(ushort),
                (ushort) 15
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "123"),
                typeof(int),
                123
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "123"),
                typeof(uint),
                123u
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "123"),
                typeof(long),
                123L
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "123"),
                typeof(ulong),
                123UL
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "123.45"),
                typeof(float),
                123.45f
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "123.45"),
                typeof(double),
                123.45
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "123.45"),
                typeof(decimal),
                123.45m
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
                new CommandOptionInput("option", "value"),
                typeof(TestStringParseableWithFormatProvider),
                TestStringParseableWithFormatProvider.Parse("value", CultureInfo.InvariantCulture)
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
                new CommandOptionInput("option", new[] {"1337", "2441"}),
                typeof(int?[]),
                new int?[] {1337, 2441}
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", new[] {"value1", "value2"}),
                typeof(TestStringConstructable[]),
                new[] {new TestStringConstructable("value1"), new TestStringConstructable("value2")}
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

            yield return new TestCaseData(
                new CommandOptionInput("option", new[] {"value1", "value2"}),
                typeof(List<string>),
                new List<string> {"value1", "value2"}
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", new[] {"value1", "value2"}),
                typeof(HashSet<string>),
                new HashSet<string> {"value1", "value2"}
            );
        }

        private static IEnumerable<TestCaseData> GetTestCases_ConvertOptionInput_Negative()
        {
            yield return new TestCaseData(
                new CommandOptionInput("option", "1234.5"),
                typeof(int)
            );

            yield return new TestCaseData(
                new CommandOptionInput("option", "123"),
                typeof(TestNonStringParseable)
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_ConvertOptionInput))]
        public void ConvertOptionInput_Test(CommandOptionInput optionInput, Type targetType,
            object expectedConvertedValue)
        {
            // Arrange
            var converter = new CommandOptionInputConverter();

            // Act
            var convertedValue = converter.ConvertOptionInput(optionInput, targetType);

            // Assert
            convertedValue.Should().BeEquivalentTo(expectedConvertedValue);
            convertedValue?.Should().BeAssignableTo(targetType);
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_ConvertOptionInput_Negative))]
        public void ConvertOptionInput_Negative_Test(CommandOptionInput optionInput, Type targetType)
        {
            // Arrange
            var converter = new CommandOptionInputConverter();

            // Act & Assert
            converter.Invoking(c => c.ConvertOptionInput(optionInput, targetType))
                .Should().ThrowExactly<InvalidCommandOptionInputException>();
        }
    }
}