using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.Utils;

internal class DiagnosticReporter(Action<Diagnostic> reportDiagnostic)
{
    internal DiagnosticReporter(IList<Diagnostic> diagnostics)
        : this(diagnostics.Add) { }

    public void Report(Diagnostic diagnostic) => reportDiagnostic(diagnostic);

    public void Report(
        DiagnosticDescriptor descriptor,
        Location? location = null,
        params object[] messageArgs
    ) => Report(Diagnostic.Create(descriptor, location, messageArgs));
}
