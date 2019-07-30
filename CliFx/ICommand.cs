using System.Threading.Tasks;
using CliFx.Services;

namespace CliFx
{
    public interface ICommand
    {
        Task ExecuteAsync(IConsole console);
    }
}