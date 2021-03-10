using System.Threading.Tasks;
using CliFx.Infrastructure;
using Newtonsoft.Json;

namespace CliFx.Tests.Utils
{
    public abstract class SelfSerializingCommandBase : ICommand
    {
        public ValueTask ExecuteAsync(IConsole console)
        {
            // TODO: use STJ?
            console.Output.WriteLine(JsonConvert.SerializeObject(this));
            return default;
        }
    }
}