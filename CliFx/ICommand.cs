using System.Threading.Tasks;
using CliFx.Models;

namespace CliFx
{
    public interface ICommand
    {
        Task<ExitCode> ExecuteAsync();
    }
}