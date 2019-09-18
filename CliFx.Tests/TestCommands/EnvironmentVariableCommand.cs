using CliFx.Attributes;
using CliFx.Services;
using System.Threading.Tasks;

namespace CliFx.Tests.TestCommands
{
	[Command(Description = "Read options from environment variables.")]
	public class EnvironmentVariableCommand : ICommand
	{
		[CommandOption("opt", EnvironmentVariableName = "OPTION_VALUE_FROM_ENV")]
		public string Option { get; set; }

		public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
	}
}
