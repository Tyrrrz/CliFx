using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
	[Command(Description = "Reads multiple option values from environment variables.")]
	public class EnvironmentVariableWithMultipleValuesCommand : ICommand
	{
		[CommandOption("opt", EnvironmentVariableName = "ENV_MULTIPLE_VALUES")]
		public IEnumerable<string>? Option { get; set; }

		public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
	}
}
