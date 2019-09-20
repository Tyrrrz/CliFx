using CliFx.Attributes;
using CliFx.Services;
using System.Threading.Tasks;

namespace CliFx.Tests.TestCommands
{
	[Command(Description = "Read option values from environment variables.")]
	public class EnvironmentVariableCommand : ICommand
	{
		[CommandOption("opt", EnvironmentVariableName = "ENV_VAR_1")]
		public string Option { get; set; }

		public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
	}
}
