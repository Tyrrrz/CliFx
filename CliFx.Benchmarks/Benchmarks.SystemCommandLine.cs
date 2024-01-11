using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace CliFx.Benchmarks;

public partial class Benchmarks
{
    public class SystemCommandLineCommand
    {
        public static int ExecuteHandler(string s, int i, bool b) => 0;

        public Task<int> ExecuteAsync(string[] args)
        {
            var command = new RootCommand
            {
                new Option(["--str", "-s"]) { Argument = new Argument<string?>() },
                new Option(["--int", "-i"]) { Argument = new Argument<int>() },
                new Option(["--bool", "-b"]) { Argument = new Argument<bool>() }
            };

            command.Handler = CommandHandler.Create(
                typeof(SystemCommandLineCommand).GetMethod(nameof(ExecuteHandler))!
            );

            return command.InvokeAsync(args);
        }
    }

    [Benchmark(Description = "System.CommandLine")]
    public async Task<int> ExecuteWithSystemCommandLine() =>
        await new SystemCommandLineCommand().ExecuteAsync(Arguments);
}
