using System;
using System.Collections.Generic;
using CliFx.Exceptions;
using CliFx.Tests.TestCommands;
using CliFx.Tests.TestCustomTypes;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests
{
    [TestFixture]
    public class DefaultCommandFactoryTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_CreateInstance()
        {
            yield return new TestCaseData(typeof(HelloWorldDefaultCommand));
        }

        private static IEnumerable<TestCaseData> GetTestCases_CreateInstance_Negative()
        {
            yield return new TestCaseData(typeof(TestNonStringParseable));
        }

        [TestCaseSource(nameof(GetTestCases_CreateInstance))]
        public void CreateInstance_Test(Type type)
        {
            // Arrange
            var activator = new DefaultTypeActivator();

            // Act
            var obj = activator.CreateInstance(type);

            // Assert
            obj.Should().BeOfType(type);
        }

        [TestCaseSource(nameof(GetTestCases_CreateInstance_Negative))]
        public void CreateInstance_Negative_Test(Type type)
        {
            // Arrange
            var activator = new DefaultTypeActivator();

            // Act & Assert
            var ex = Assert.Throws<CliFxException>(() => activator.CreateInstance(type));
            Console.WriteLine(ex.Message);
        }
    }
}