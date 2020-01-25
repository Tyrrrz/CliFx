using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests.TestCommands
{
	[Command(Description = "Reads option values from environment variables.")]
	public class EnvironmentVariableCommand : ICommand
	{
		[CommandOption("opt", EnvironmentVariableName = "ENV_SINGLE_VALUE")]
		public string? Option { get; set; }

		public ValueTask ExecuteAsync(IConsole console) => default;
	}
}
