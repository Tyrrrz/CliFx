using CliFx.Attributes;
using CliFx.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CliFx.Tests.TestCommands
{
	[Command(Description = "Read multiple option values from environment variables.")]
	public class EnvironmentVariableWithMultipleValuesCommand : ICommand
	{
		[CommandOption("opt", EnvironmentVariableName = "ENV_VAR_2")]
		public IEnumerable<string> Option { get; set; }

		public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
	}
}
