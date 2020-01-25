using System;
using System.Collections.Generic;
using CliFx.Tests.TestCommands;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests
{
    [TestFixture]
    public class DelegateCommandFactoryTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_CreateCommand()
        {
            yield return new TestCaseData(
                new Func<Type, object>(Activator.CreateInstance),
                typeof(HelloWorldDefaultCommand)
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_CreateCommand))]
        public void CreateCommand_Test(Func<Type, object> activatorFunc, Type type)
        {
            // Arrange
            var activator = new DelegateTypeActivator(activatorFunc);

            // Act
            var obj = activator.CreateInstance(type);

            // Assert
            obj.Should().BeOfType(type);
        }
    }
}