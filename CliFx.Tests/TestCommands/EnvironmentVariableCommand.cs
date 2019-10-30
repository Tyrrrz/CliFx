using System.Threading;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
	[Command(Description = "Reads option values from environment variables.")]
	public class EnvironmentVariableCommand : ICommand
	{
		[CommandOption("opt", EnvironmentVariableName = "ENV_SINGLE_VALUE")]
		public string Option { get; set; }

		public Task ExecuteAsync(IConsole console, CancellationToken cancellationToken) => Task.CompletedTask;
	}
}
