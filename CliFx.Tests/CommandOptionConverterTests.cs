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
            yield return new TestCaseData("value", typeof(string), "value")
                .SetName("To string");

            yield return new TestCaseData("value", typeof(object), "value")
                .SetName("To object");

            yield return new TestCaseData("true", typeof(bool), true)
                .SetName("To bool (true)");

            yield return new TestCaseData("false", typeof(bool), false)
                .SetName("To bool (false)");

            yield return new TestCaseData(null, typeof(bool), true)
                .SetName("To bool (switch)");

            yield return new TestCaseData("123", typeof(int), 123)
                .SetName("To int");

            yield return new TestCaseData("123.45", typeof(double), 123.45)
                .SetName("To double");

            yield return new TestCaseData("28 Apr 1995", typeof(DateTime), new DateTime(1995, 04, 28))
                .SetName("To DateTime");

            yield return new TestCaseData("28 Apr 1995", typeof(DateTimeOffset), new DateTimeOffset(new DateTime(1995, 04, 28)))
                .SetName("To DateTimeOffset");

            yield return new TestCaseData("00:14:59", typeof(TimeSpan), new TimeSpan(00, 14, 59))
                .SetName("To TimeSpan");

            yield return new TestCaseData("value2", typeof(TestEnum), TestEnum.Value2)
                .SetName("To enum");

            yield return new TestCaseData("666", typeof(int?), 666)
                .SetName("To int? (with value)");

            yield return new TestCaseData(null, typeof(int?), null)
                .SetName("To int? (no value)");

            yield return new TestCaseData("value3", typeof(TestEnum?), TestEnum.Value3)
                .SetName("To enum? (with value)");

            yield return new TestCaseData(null, typeof(TestEnum?), null)
                .SetName("To enum? (no value)");

            yield return new TestCaseData("01:00:00", typeof(TimeSpan?), new TimeSpan(01, 00, 00))
                .SetName("To TimeSpan? (with value)");

            yield return new TestCaseData(null, typeof(TimeSpan?), null)
                .SetName("To TimeSpan? (no value)");
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