using System.Threading.Tasks;

namespace CliFx.Extensibility
{
    public interface IDirectiveProcessor
    {
        ValueTask<int?> HandleAsync();
    }
}