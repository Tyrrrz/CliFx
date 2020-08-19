using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests.Commands
{
    [Command("cmd")]
    public class WithDependenciesCommand : ICommand
    {
        public class DependencyA
        {
        }

        public class DependencyB
        {
        }

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