using Microsoft.CodeAnalysis;

namespace CliFx.Analyzers
{
    public static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor CliFx0001 =
            new(nameof(CliFx0001),
                "Type must implement the 'CliFx.ICommand' interface in order to be a valid command",
                "Type must implement the 'CliFx.ICommand' interface in order to be a valid command",
                "Usage", DiagnosticSeverity.Error, true
            );

        public static readonly DiagnosticDescriptor CliFx0002 =
            new(nameof(CliFx0002),
                "Type must be annotated with the 'CliFx.Attributes.CommandAttribute' in order to be a valid command",
                "Type must be annotated with the 'CliFx.Attributes.CommandAttribute' in order to be a valid command",
                "Usage", DiagnosticSeverity.Error, true
            );

        public static readonly DiagnosticDescriptor CliFx0021 =
            new(nameof(CliFx0021),
                "Parameter order must be unique within its command",
                "Parameter order must be unique within its command",
                "Usage", DiagnosticSeverity.Error, true
            );

        public static readonly DiagnosticDescriptor CliFx0022 =
            new(nameof(CliFx0022),
                "Parameter order must have unique name within its command",
                "Parameter order must have unique name within its command",
                "Usage", DiagnosticSeverity.Error, true
            );

        public static readonly DiagnosticDescriptor CliFx0023 =
            new(nameof(CliFx0023),
                "Only one non-scalar parameter per command is allowed",
                "Only one non-scalar parameter per command is allowed",
                "Usage", DiagnosticSeverity.Error, true
            );

        public static readonly DiagnosticDescriptor CliFx0024 =
            new(nameof(CliFx0024),
                "Non-scalar parameter must be last in order",
                "Non-scalar parameter must be last in order",
                "Usage", DiagnosticSeverity.Error, true
            );

        public static readonly DiagnosticDescriptor CliFx0025 =
            new(nameof(CliFx0025),
                "Parameter converter must implement 'CliFx.IArgumentValueConverter'",
                "Parameter converter must implement 'CliFx.IArgumentValueConverter'",
                "Usage", DiagnosticSeverity.Error, true
            );

        public static readonly DiagnosticDescriptor CliFx0026 =
            new(nameof(CliFx0026),
                "Parameter validator must implement 'CliFx.ArgumentValueValidator<T>'",
                "Parameter validator must implement 'CliFx.ArgumentValueValidator<T>'",
                "Usage", DiagnosticSeverity.Error, true
            );

        public static readonly DiagnosticDescriptor CliFx0041 =
            new(nameof(CliFx0041),
                "Option must have a name or short name specified",
                "Option must have a name or short name specified",
                "Usage", DiagnosticSeverity.Error, true
            );

        public static readonly DiagnosticDescriptor CliFx0042 =
            new(nameof(CliFx0042),
                "Option name must be at least 2 characters long",
                "Option name must be at least 2 characters long",
                "Usage", DiagnosticSeverity.Error, true
            );

        public static readonly DiagnosticDescriptor CliFx0043 =
            new(nameof(CliFx0043),
                "Option name must be unique within its command",
                "Option name must be unique within its command",
                "Usage", DiagnosticSeverity.Error, true
            );

        public static readonly DiagnosticDescriptor CliFx0044 =
            new(nameof(CliFx0044),
                "Option short name must be unique within its command",
                "Option short name must be unique within its command",
                "Usage", DiagnosticSeverity.Error, true
            );

        public static readonly DiagnosticDescriptor CliFx0045 =
            new(nameof(CliFx0045),
                "Option environment variable name must be unique within its command",
                "Option environment variable name must be unique within its command",
                "Usage", DiagnosticSeverity.Error, true
            );

        public static readonly DiagnosticDescriptor CliFx0046 =
            new(nameof(CliFx0046),
                "Option converter must implement 'CliFx.IArgumentValueConverter'",
                "Option converter must implement 'CliFx.IArgumentValueConverter'",
                "Usage", DiagnosticSeverity.Error, true
            );

        public static readonly DiagnosticDescriptor CliFx0047 =
            new(nameof(CliFx0047),
                "Option validator must implement 'CliFx.ArgumentValueValidator<T>'",
                "Option validator must implement 'CliFx.ArgumentValueValidator<T>'",
                "Usage", DiagnosticSeverity.Error, true
            );

        public static readonly DiagnosticDescriptor CliFx0048 =
            new(nameof(CliFx0048),
                "Option name must begin with a letter character.",
                "Option name must begin with a letter character.",
                "Usage", DiagnosticSeverity.Error, true
            );

        public static readonly DiagnosticDescriptor CliFx0049 =
            new(nameof(CliFx0049),
                "Option short name must be a letter character.",
                "Option short name must be a letter character.",
                "Usage", DiagnosticSeverity.Error, true
            );

        public static readonly DiagnosticDescriptor CliFx0100 =
            new(nameof(CliFx0100),
                "Use the provided IConsole abstraction instead of System.Console to ensure that the command can be tested in isolation",
                "Use the provided IConsole abstraction instead of System.Console to ensure that the command can be tested in isolation",
                "Usage", DiagnosticSeverity.Warning, true
            );
    }
}