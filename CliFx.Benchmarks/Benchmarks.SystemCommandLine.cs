using System.CommandLine;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace CliFx.Benchmarks;

public partial class Benchmarks
{
    public class SystemCommandLineCommand
    {
        public static void ExecuteHandler(string s, int i, bool b) { }

        public Task<int> ExecuteAsync(string[] args)
        {
            var stringOption = new Option<string>(["--str", "-s"]);
            var intOption = new Option<int>(["--int", "-i"]);
            var boolOption = new Option<bool>(["--bool", "-b"]);

            var command = new RootCommand();
            command.AddOption(stringOption);
            command.AddOption(intOption);
            command.AddOption(boolOption);

            command.SetHandler(ExecuteHandler, stringOption, intOption, boolOption);

            return command.InvokeAsync(args);
        }
    }

    [Benchmark(Description = "System.CommandLine")]
    public async Task<int> ExecuteWithSystemCommandLine() =>
        await new SystemCommandLineCommand().ExecuteAsync(Arguments);
}
