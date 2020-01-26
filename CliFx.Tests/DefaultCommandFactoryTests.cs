using System;
using System.Collections.Generic;
using CliFx.Tests.TestCommands;
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
    }
}