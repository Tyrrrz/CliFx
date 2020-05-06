using CliFx.Attributes;
using System.Threading.Tasks;

namespace CliFx.Tests.Dummy.Commands
{
    public enum Values
    {
        Value1, Value2, Value3, Value4, Value5, Value6,
        Value7, Value8, Value9, Value10, Value11, Value12,
    }

    public enum EmptyEnum
    {

    }

    [Command("enum")]
    public class ShowValidEnumValuesCommand : ICommand
    {
        [CommandOption("value", Description = "Enum option.", IsRequired = true)]
        public Values Value { get; set; } = Values.Value1;

        [CommandOption("empty", Description = "Empty enum option.")]
        public EmptyEnum Empty { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}