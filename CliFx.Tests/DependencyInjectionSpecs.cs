using CliFx.Exceptions;
using CliFx.Tests.Commands;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class DependencyInjectionSpecs
    {
        private readonly ITestOutputHelper _output;

        public DependencyInjectionSpecs(ITestOutputHelper output) => _output = output;

        [Fact]
        public void Default_type_activator_can_initialize_a_command_if_it_has_a_parameterless_constructor()
        {
            // Arrange
            var activator = new DefaultTypeActivator();

            // Act
            var obj = activator.CreateInstance(typeof(DefaultCommand));

            // Assert
            obj.Should().BeOfType<DefaultCommand>();
        }

        [Fact]
        public void Default_type_activator_cannot_initialize_a_command_if_it_does_not_have_a_parameterless_constructor()
        {
            // Arrange
            var activator = new DefaultTypeActivator();

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => activator.CreateInstance(typeof(WithDependenciesCommand)));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Delegate_type_activator_can_initialize_a_command_using_a_custom_function()
        {
            // Arrange
            var activator = new DelegateTypeActivator(_ =>
                new WithDependenciesCommand(
                    new WithDependenciesCommand.DependencyA(),
                    new WithDependenciesCommand.DependencyB())
            );

            // Act
            var obj = activator.CreateInstance(typeof(WithDependenciesCommand));

            // Assert
            obj.Should().BeOfType<WithDependenciesCommand>();
        }

        [Fact]
        public void Delegate_type_activator_throws_if_the_underlying_function_returns_null()
        {
            // Arrange
            var activator = new DelegateTypeActivator(_ => null!);

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => activator.CreateInstance(typeof(WithDependenciesCommand)));
            _output.WriteLine(ex.Message);
        }
    }
}