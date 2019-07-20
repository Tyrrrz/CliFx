using System.Threading.Tasks;
using CliFx.Models;

namespace CliFx
{
    public interface ICommand
    {
        CommandContext Context { get; set; }

        Task<ExitCode> ExecuteAsync();
    }
}