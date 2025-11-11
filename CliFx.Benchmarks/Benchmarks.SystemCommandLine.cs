using System.CommandLine;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace CliFx.Benchmarks;

public partial class Benchmarks
{
    public class SystemCommandLineCommand
    {
        public Task<int> ExecuteAsync(string[] args)
        {
            var stringOption = new Option<string>("--str", "-s");
            var intOption = new Option<int>("--int", "-i");
            var boolOption = new Option<bool>("--bool", "-b");

            var command = new RootCommand { stringOption, intOption, boolOption };

            command.SetAction(r =>
            {
                _ = r.GetValue(stringOption);
                _ = r.GetValue(intOption);
                _ = r.GetValue(boolOption);
            });

            return command.Parse(args).InvokeAsync();
        }
    }

    [Benchmark(Description = "System.CommandLine")]
    public async Task<int> ExecuteWithSystemCommandLine() =>
        await new SystemCommandLineCommand().ExecuteAsync(Arguments);
}
