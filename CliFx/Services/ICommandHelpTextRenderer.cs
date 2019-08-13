using CliFx.Models;

namespace CliFx.Services
{
    public interface ICommandHelpTextRenderer
    {
        void RenderHelpText(IConsole console, HelpTextSource source);
    }
}