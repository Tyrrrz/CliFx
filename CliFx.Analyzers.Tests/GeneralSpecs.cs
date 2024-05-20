using System;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests;

public class GeneralSpecs
{
    [Fact]
    public void All_analyzers_have_unique_diagnostic_IDs()
    {
        // Arrange
        var analyzers = typeof(AnalyzerBase)
            .Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(DiagnosticAnalyzer)))
            .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t)!)
            .ToArray();

        // Act
        var diagnosticIds = analyzers
            .SelectMany(a => a.SupportedDiagnostics.Select(d => d.Id))
            .ToArray();

        // Assert
        diagnosticIds.Should().OnlyHaveUniqueItems();
    }
}
