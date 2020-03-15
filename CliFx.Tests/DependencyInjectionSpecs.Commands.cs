using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests
{
    public partial class DependencyInjectionSpecs
    {
        [Command]
        private class WithoutDependenciesCommand : ICommand
        {
            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        private class DependencyA
        {
        }

        private class DependencyB
        {
        }

        [Command]
        private class WithDependenciesCommand : ICommand
        {
            private readonly DependencyA _dependencyA;
            private readonly DependencyB _dependencyB;

            public WithDependenciesCommand(DependencyA dependencyA, DependencyB dependencyB)
            {
                _dependencyA = dependencyA;
                _dependencyB = dependencyB;
            }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }
    }
}