using McMaster.Extensions.CommandLineUtils;

namespace CliFx.Benchmarks.Commands
{
    public class McMasterCommand
    {
        [Option("--str|-s")]
        public string StrOption { get; set; }

        [Option("--int|-i")]
        public int IntOption { get; set; }

        [Option("--bool|-b")]
        public bool BoolOption { get; set; }

        public int OnExecute() => 0;
    }
}