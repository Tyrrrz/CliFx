using Microsoft.CodeAnalysis;

namespace CliFx.Analyzers
{
    public static class Descriptor
    {
        public static readonly DiagnosticDescriptor CliFx0001 =
            new DiagnosticDescriptor(nameof(CliFx0001),
                "Avoid using System.Console in commands",
                "Use the provided IConsole abstraction instead of System.Console to ensure that the command can be tested in isolation.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0002 =
            new DiagnosticDescriptor(nameof(CliFx0002),
                "Command type must be annotated by an attribute",
                "Annotate the type with 'CliFx.Attributes.CommandAttribute' in order for it to be a valid command.",
                "Usage", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor CliFx0003 =
            new DiagnosticDescriptor(nameof(CliFx0003),
                "Command type must implement an interface",
                "Ensure the type implements 'CliFx.ICommand' in order for it to be a valid command.",
                "Usage", DiagnosticSeverity.Error, true);
    }
}