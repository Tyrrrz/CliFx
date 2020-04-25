﻿using Microsoft.CodeAnalysis;

namespace CliFx.Analyzers
{
    public static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor CliFx0001 =
            new DiagnosticDescriptor(nameof(CliFx0001),
                "Type must implement the 'CliFx.ICommand' interface in order to be a valid command.",
                "Type must implement the 'CliFx.ICommand' interface in order to be a valid command.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0002 =
            new DiagnosticDescriptor(nameof(CliFx0002),
                "Type must be annotated with the 'CliFx.Attributes.CommandAttribute' in order to be a valid command.",
                "Type must be annotated with the 'CliFx.Attributes.CommandAttribute' in order to be a valid command.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0021 =
            new DiagnosticDescriptor(nameof(CliFx0021),
                "Parameter order must be unique within its command.",
                "Parameter order must be unique within its command.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0022 =
            new DiagnosticDescriptor(nameof(CliFx0022),
                "Parameter order must have unique name within its command.",
                "Parameter order must have unique name within its command.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0023 =
            new DiagnosticDescriptor(nameof(CliFx0023),
                "Only one non-scalar parameter per command is allowed.",
                "Only one non-scalar parameter per command is allowed.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0024 =
            new DiagnosticDescriptor(nameof(CliFx0024),
                "Non-scalar parameter must be last in order.",
                "Non-scalar parameter must be last in order.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0041 =
            new DiagnosticDescriptor(nameof(CliFx0041),
                "Option must have a name or short name specified.",
                "Option must have a name or short name specified.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0042 =
            new DiagnosticDescriptor(nameof(CliFx0042),
                "Option name must be at least 2 characters long.",
                "Option name must be at least 2 characters long.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0043 =
            new DiagnosticDescriptor(nameof(CliFx0043),
                "Option name must be unique within its command.",
                "Option name must be unique within its command.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0044 =
            new DiagnosticDescriptor(nameof(CliFx0044),
                "Option short name must be unique within its command.",
                "Option short name must be unique within its command.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0045 =
            new DiagnosticDescriptor(nameof(CliFx0045),
                "Option environment variable name must be unique within its command.",
                "Option environment variable name must be unique within its command.",
                "Usage", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor CliFx0100 =
            new DiagnosticDescriptor(nameof(CliFx0100),
                "Avoid using System.Console in commands.",
                "Use the provided IConsole abstraction instead of System.Console to ensure that the command can be tested in isolation.",
                "Usage", DiagnosticSeverity.Warning, true);
    }
}