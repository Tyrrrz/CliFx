using Microsoft.CodeAnalysis;

namespace CliFx.Analyzers
{
    public static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor CliFx0001 =
            new DiagnosticDescriptor(nameof(CliFx0001),
                "Command type must implement an interface",
                "Ensure the type implements 'CliFx.ICommand' in order for it to be a valid command.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0002 =
            new DiagnosticDescriptor(nameof(CliFx0002),
                "Command type must be annotated by an attribute",
                "Annotate the type with 'CliFx.Attributes.CommandAttribute' in order for it to be a valid command.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0021 =
            new DiagnosticDescriptor(nameof(CliFx0021),
                "Command parameters must have unique order",
                "Ensure that have command parameters have different order specified.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0022 =
            new DiagnosticDescriptor(nameof(CliFx0022),
                "Command parameters must have unique names",
                "Ensure that have command parameters have different names specified.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0023 =
            new DiagnosticDescriptor(nameof(CliFx0023),
                "Only one non-scalar parameter is allowed",
                "TODO.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0024 =
            new DiagnosticDescriptor(nameof(CliFx0024),
                "Non-scalar parameter must be last in order",
                "TODO.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0041 =
            new DiagnosticDescriptor(nameof(CliFx0041),
                "Options must have a non-empty name",
                "TODO.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0042 =
            new DiagnosticDescriptor(nameof(CliFx0042),
                "Options must have a name of 2 or more characters",
                "TODO.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0043 =
            new DiagnosticDescriptor(nameof(CliFx0043),
                "Options must have unique names",
                "TODO.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0044 =
            new DiagnosticDescriptor(nameof(CliFx0044),
                "Options must have unique short names",
                "TODO.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0045 =
            new DiagnosticDescriptor(nameof(CliFx0045),
                "Options must have unique environment variable names",
                "TODO.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0100 =
            new DiagnosticDescriptor(nameof(CliFx0100),
                "Avoid using System.Console in commands",
                "Use the provided IConsole abstraction instead of System.Console to ensure that the command can be tested in isolation.",
                "Usage", DiagnosticSeverity.Warning, true);
    }
}