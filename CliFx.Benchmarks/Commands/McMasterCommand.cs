namespace CliFx.Benchmarks.Commands
{
    using McMaster.Extensions.CommandLineUtils;

    public class McMasterCommand
    {
        [Option("--str|-s")]
        public string? StrOption { get; set; }

        [Option("--int|-i")]
        public int IntOption { get; set; }

        [Option("--bool|-b")]
        public bool BoolOption { get; set; }

        public int OnExecute()
        {
            return 0;
        }
    }
}