using System;
using System.Collections.Generic;
using CliFx.Services;
using CliFx.Tests.TestObjects;
using NUnit.Framework;

namespace CliFx.Tests
{
    [TestFixture]
    public class CommandOptionConverterTests
    {
        private static IEnumerable<TestCaseData> GetData_ConvertOption()
        {
            yield return new TestCaseData("value", typeof(string), "value");

            yield return new TestCaseData("value", typeof(object), "value");

            yield return new TestCaseData("true", typeof(bool), true);

            yield return new TestCaseData("false", typeof(bool), false);

            yield return new TestCaseData(null, typeof(bool), true);

            yield return new TestCaseData("123", typeof(int), 123);

            yield return new TestCaseData("123.45", typeof(double), 123.45);

            yield return new TestCaseData("28 Apr 1995", typeof(DateTime), new DateTime(1995, 04, 28));

            yield return new TestCaseData("28 Apr 1995", typeof(DateTimeOffset), new DateTimeOffset(new DateTime(1995, 04, 28)));

            yield return new TestCaseData("00:14:59", typeof(TimeSpan), new TimeSpan(00, 14, 59));

            yield return new TestCaseData("value2", typeof(TestEnum), TestEnum.Value2);

            yield return new TestCaseData("666", typeof(int?), 666);

            yield return new TestCaseData(null, typeof(int?), null);

            yield return new TestCaseData("value3", typeof(TestEnum?), TestEnum.Value3);

            yield return new TestCaseData(null, typeof(TestEnum?), null);

            yield return new TestCaseData("01:00:00", typeof(TimeSpan?), new TimeSpan(01, 00, 00));

            yield return new TestCaseData(null, typeof(TimeSpan?), null);
        }

        [Test]
        [TestCaseSource(nameof(GetData_ConvertOption))]
        public void ConvertOption_Test(string value, Type targetType, object expectedConvertedValue)
        {
            // Arrange
            var converter = new CommandOptionConverter();

            // Act
            var convertedValue = converter.ConvertOption(value, targetType);

            // Assert
            Assert.That(convertedValue, Is.EqualTo(expectedConvertedValue));

            if (convertedValue != null)
                Assert.That(convertedValue, Is.AssignableTo(targetType));
        }
    }
}