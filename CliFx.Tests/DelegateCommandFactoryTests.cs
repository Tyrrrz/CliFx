using System;
using System.Collections.Generic;
using CliFx.Exceptions;
using CliFx.Tests.TestCommands;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests
{
    [TestFixture]
    public class DelegateCommandFactoryTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_CreateInstance()
        {
            yield return new TestCaseData(
                new Func<Type, object>(Activator.CreateInstance),
                typeof(HelloWorldDefaultCommand)
            );
        }

        private static IEnumerable<TestCaseData> GetTestCases_CreateInstance_Negative()
        {
            yield return new TestCaseData(
                new Func<Type, object>(_ => null),
                typeof(HelloWorldDefaultCommand)
            );
        }

        [TestCaseSource(nameof(GetTestCases_CreateInstance))]
        public void CreateInstance_Test(Func<Type, object> activatorFunc, Type type)
        {
            // Arrange
            var activator = new DelegateTypeActivator(activatorFunc);

            // Act
            var obj = activator.CreateInstance(type);

            // Assert
            obj.Should().BeOfType(type);
        }

        [TestCaseSource(nameof(GetTestCases_CreateInstance_Negative))]
        public void CreateInstance_Negative_Test(Func<Type, object> activatorFunc, Type type)
        {
            // Arrange
            var activator = new DelegateTypeActivator(activatorFunc);

            // Act & Assert
            var ex = Assert.Throws<CliFxException>(() => activator.CreateInstance(type));
            Console.WriteLine(ex.Message);
        }
    }
}