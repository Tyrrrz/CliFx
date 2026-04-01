using System.Linq;
using Basic.Reference.Assemblies;
using CliFx.Generators;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public class ProgramEntryPointGeneratorSpecs(ITestOutputHelper testOutput) : SpecsBase(testOutput)
{
    private static GeneratorDriverRunResult RunGenerator(string sourceCode, OutputKind outputKind)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode, parseOptions);

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            Net100.References.All.Append(
                MetadataReference.CreateFromFile(typeof(ICommand).Assembly.Location)
            ),
            new CSharpCompilationOptions(outputKind)
        );

        return CSharpGeneratorDriver
            .Create(
                [new ProgramEntryPointGenerator().AsSourceGenerator()],
                parseOptions: parseOptions
            )
            .RunGenerators(compilation)
            .GetRunResult();
    }

    [Fact]
    public void Entry_point_is_generated_for_console_app_without_an_existing_entry_point()
    {
        // Act
        var result = RunGenerator("", OutputKind.ConsoleApplication);

        // Assert
        result
            .Results.SelectMany(r => r.GeneratedSources)
            .Should()
            .ContainSingle(s => s.HintName == "Program.g.cs");
    }

    [Fact]
    public void Entry_point_is_generated_for_windows_app_without_an_existing_entry_point()
    {
        // Act
        var result = RunGenerator("", OutputKind.WindowsApplication);

        // Assert
        result
            .Results.SelectMany(r => r.GeneratedSources)
            .Should()
            .ContainSingle(s => s.HintName == "Program.g.cs");
    }

    [Fact]
    public void Entry_point_is_not_generated_when_an_explicit_main_method_exists()
    {
        // Arrange
        // lang=csharp
        var sourceCode = """
            public class Program
            {
                public static int Main() => 0;
            }
            """;

        // Act
        var result = RunGenerator(sourceCode, OutputKind.ConsoleApplication);

        // Assert
        result
            .Results.SelectMany(r => r.GeneratedSources)
            .Should()
            .NotContain(s => s.HintName == "Program.g.cs");
    }

    [Fact]
    public void Entry_point_is_not_generated_when_top_level_statements_are_present()
    {
        // Arrange
        // lang=csharp
        var sourceCode = "return 0;";

        // Act
        var result = RunGenerator(sourceCode, OutputKind.ConsoleApplication);

        // Assert
        result
            .Results.SelectMany(r => r.GeneratedSources)
            .Should()
            .NotContain(s => s.HintName == "Program.g.cs");
    }

    [Fact]
    public void Entry_point_is_not_generated_for_library_projects()
    {
        // Act
        var result = RunGenerator("", OutputKind.DynamicallyLinkedLibrary);

        // Assert
        result
            .Results.SelectMany(r => r.GeneratedSources)
            .Should()
            .NotContain(s => s.HintName == "Program.g.cs");
    }
}
