using System;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using CliFx.Tests.Commands;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class TypeActivationSpecs : SpecsBase
    {
        public TypeActivationSpecs(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        [Fact]
        public void Default_type_activator_can_initialize_a_type_if_it_has_a_parameterless_constructor()
        {
            // Arrange
            var activator = new DefaultTypeActivator();

            // Act
            var obj = activator.CreateInstance(typeof(NoOpCommand));

            // Assert
            obj.Should().BeOfType<NoOpCommand>();
        }

        [Fact]
        public void Default_type_activator_cannot_initialize_a_type_if_it_does_not_have_a_parameterless_constructor()
        {
            // Arrange
            var activator = new DefaultTypeActivator();

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => activator.CreateInstance(typeof(CliApplication)));
            TestOutput.WriteLine(ex.Message);
        }

        [Fact]
        public void Delegate_type_activator_can_initialize_a_type_using_a_custom_function()
        {
            // Arrange
            var activator = new DelegateTypeActivator(t => Activator.CreateInstance(t)!);

            // Act
            var obj = activator.CreateInstance(typeof(NoOpCommand));

            // Assert
            obj.Should().BeOfType<NoOpCommand>();
        }

        [Fact]
        public void Delegate_type_activator_throws_if_the_underlying_function_returns_null()
        {
            // Arrange
            var activator = new DelegateTypeActivator(_ => null!);

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => activator.CreateInstance(typeof(NoOpCommand)));
            TestOutput.WriteLine(ex.Message);
        }
    }
}