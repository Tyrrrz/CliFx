using CliFx.Models;

namespace CliFx.Services
{
    public interface IHelpTextBuilder
    {
        string Build(CommandContext context);
    }
}