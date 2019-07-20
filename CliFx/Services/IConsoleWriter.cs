using CliFx.Models;

namespace CliFx.Services
{
    public interface IConsoleWriter
    {
        void Write(TextSpan text);

        void WriteLine(TextSpan text);
    }
}