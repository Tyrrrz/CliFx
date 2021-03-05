using System.Threading.Tasks;
using CliFx.Infrastructure;
using Newtonsoft.Json;

namespace CliFx.Tests.Commands
{
    public abstract class SelfSerializeCommandBase : ICommand
    {
        public ValueTask ExecuteAsync(IConsole console)
        {
            console.Output.WriteLine(JsonConvert.SerializeObject(this));
            return default;
        }
    }
}