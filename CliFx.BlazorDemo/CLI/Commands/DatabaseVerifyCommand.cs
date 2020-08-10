using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.BlazorDemo.CLI.Services;

namespace CliFx.BlazorDemo.CLI.Commands
{
    [Command("database verify", Description = "Migrates the database.")]
    public class DatabaseVerifyCommand : ICommand
    {
        private readonly ICliContext _cliContext;

        public DatabaseVerifyCommand(ICliContext cliContext)
        {
            _cliContext = cliContext;
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            await console.Output.WriteLineAsync("This will normally verify EF Core migrations.");

            if(_cliContext.CurrentInput.HasDirective("hidden")) //TODO: remove this; add AddCustomDirective<T>(string name) to builder; add AddCustomDirectivesFromAssembly; add AddCustomDirectivesFromThisAssembly
            {
                console.Output.WriteLine("Hello World!");
            }
        }
    }
}
