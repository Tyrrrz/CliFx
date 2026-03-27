using CliFx.Tests.Utils;

namespace CliFx.Tests.Utils.Extensions;

internal static class CommandLineApplicationBuilderExtensions
{
    extension(CommandLineApplicationBuilder builder)
    {
        public CommandLineApplicationBuilder AddCommand(string sourceCode) =>
            builder.AddCommand(CommandCompiler.Compile(sourceCode));

        public CommandLineApplicationBuilder AddCommands(string sourceCode) =>
            builder.AddCommands(CommandCompiler.CompileMany(sourceCode));
    }
}
