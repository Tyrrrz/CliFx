using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CliFx.Attributes;
using CliFx.Infrastructure;
using CliFx.Schema;

namespace CliFx.Benchmarks;

public partial class Benchmarks
{
    [Command]
    public class CliFxCommand : ICommand
    {
        [CommandOption("str", 's')]
        public string? StrOption { get; set; }

        [CommandOption("int", 'i')]
        public int IntOption { get; set; }

        [CommandOption("bool", 'b')]
        public bool BoolOption { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }

    private static readonly CommandSchema CliFxCommandSchema = new CommandSchema(
        typeof(CliFxCommand),
        null,
        null,
        [],
        [
            new CommandOptionSchema(
                new PropertyBinding(
                    typeof(string),
                    o => ((CliFxCommand)o).StrOption,
                    (o, v) => ((CliFxCommand)o).StrOption = (string?)v
                ),
                false,
                "str",
                's',
                null,
                false,
                null,
                null,
                []
            ),
            new CommandOptionSchema(
                new PropertyBinding(
                    typeof(int),
                    o => ((CliFxCommand)o).IntOption,
                    (o, v) => ((CliFxCommand)o).IntOption = (int)v!
                ),
                false,
                "int",
                'i',
                null,
                false,
                null,
                null,
                []
            ),
            new CommandOptionSchema(
                new PropertyBinding(
                    typeof(bool),
                    o => ((CliFxCommand)o).BoolOption,
                    (o, v) => ((CliFxCommand)o).BoolOption = (bool)v!
                ),
                false,
                "bool",
                'b',
                null,
                false,
                null,
                null,
                []
            ),
            CommandOptionSchema.ImplicitHelpOption,
            CommandOptionSchema.ImplicitVersionOption,
        ]
    );

    [Benchmark(Description = "CliFx", Baseline = true)]
    public async ValueTask<int> ExecuteWithCliFx() =>
        await new CliApplicationBuilder()
            .AddCommand(CliFxCommandSchema)
            .Build()
            .RunAsync(Arguments, new Dictionary<string, string>());
}
