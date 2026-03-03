using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Basic.Reference.Assemblies;
using CliFx.Attributes;
using CliFx.Extensibility;
using CliFx.Schema;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace CliFx.Tests.Utils;

// This class uses Roslyn to compile commands dynamically.
// It allows us to collocate commands with tests more easily, which helps a lot when reasoning about them.
// Unfortunately, this comes at a cost of static typing, but this is still a worthwhile trade off.
// Maybe one day C# will allow declaring classes inside methods and doing this will no longer be necessary.
// Language proposal: https://github.com/dotnet/csharplang/discussions/130
internal static class DynamicCommandBuilder
{
    private static bool IsRequired(PropertyInfo property) =>
        property
            .GetCustomAttributes()
            .Any(a =>
                string.Equals(
                    a.GetType().FullName,
                    "System.Runtime.CompilerServices.RequiredMemberAttribute",
                    StringComparison.Ordinal
                )
            );

    private static Type? TryGetEnumerableUnderlyingType(Type type)
    {
        if (type.IsPrimitive)
            return null;

        if (type == typeof(IEnumerable))
            return typeof(object);

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return type.GetGenericArguments().FirstOrDefault();

        return type.GetInterfaces()
            .Select(TryGetEnumerableUnderlyingType)
            .Where(t => t is not null)
            .MaxBy(t => t != typeof(object));
    }

    private static PropertyBinding CreatePropertyBinding(PropertyInfo property) =>
        new(
            property.PropertyType,
            instance => property.GetValue(instance),
            (instance, value) => property.SetValue(instance, value)
        );

    private static IBindingConverter? CreateConverter(Type? converterType)
    {
        if (converterType is null)
            return null;
        return (IBindingConverter)Activator.CreateInstance(converterType)!;
    }

    private static IReadOnlyList<IBindingValidator> CreateValidators(
        IReadOnlyList<Type> validatorTypes
    )
    {
        var validators = new IBindingValidator[validatorTypes.Count];
        for (var i = 0; i < validatorTypes.Count; i++)
            validators[i] = (IBindingValidator)Activator.CreateInstance(validatorTypes[i])!;
        return validators;
    }

    private static CommandParameterSchema? TryResolveParameter(PropertyInfo property)
    {
        var attribute = property.GetCustomAttribute<CommandParameterAttribute>();
        if (attribute is null)
            return null;

        var name = attribute.Name?.Trim() ?? property.Name.ToLowerInvariant();
        var isRequired = attribute.IsRequired || IsRequired(property);
        var description = attribute.Description?.Trim();
        var isSequence =
            property.PropertyType != typeof(string)
            && TryGetEnumerableUnderlyingType(property.PropertyType) is not null;

        return new CommandParameterSchema(
            CreatePropertyBinding(property),
            isSequence,
            attribute.Order,
            name,
            isRequired,
            description,
            CreateConverter(attribute.Converter),
            CreateValidators(attribute.Validators)
        );
    }

    private static CommandOptionSchema? TryResolveOption(PropertyInfo property)
    {
        var attribute = property.GetCustomAttribute<CommandOptionAttribute>();
        if (attribute is null)
            return null;

        var name = attribute.Name?.TrimStart('-').Trim();
        var environmentVariable = attribute.EnvironmentVariable?.Trim();
        var isRequired = attribute.IsRequired || IsRequired(property);
        var description = attribute.Description?.Trim();
        var isSequence =
            property.PropertyType != typeof(string)
            && TryGetEnumerableUnderlyingType(property.PropertyType) is not null;

        return new CommandOptionSchema(
            CreatePropertyBinding(property),
            isSequence,
            name,
            attribute.ShortName,
            environmentVariable,
            isRequired,
            description,
            CreateConverter(attribute.Converter),
            CreateValidators(attribute.Validators)
        );
    }

    public static CommandSchema BuildSchema(Type type)
    {
        var attribute = type.GetCustomAttribute<CommandAttribute>();
        var name = attribute?.Name?.Trim();
        var description = attribute?.Description?.Trim();

        var properties = type.GetProperties()
            .Union(
                type.GetInterfaces()
                    .Where(i => i != typeof(ICommand) && i.IsAssignableTo(typeof(ICommand)))
                    .SelectMany(i =>
                        i.GetProperties()
                            .Where(p =>
                                p.GetMethod is not null
                                && !p.GetMethod.IsAbstract
                                && p.SetMethod is not null
                                && !p.SetMethod.IsAbstract
                            )
                    )
            )
            .ToArray();

        var parameterSchemas = properties
            .Select(TryResolveParameter)
            .Where(p => p is not null)
            .Select(p => p!)
            .ToArray();

        var optionSchemas = properties
            .Select(TryResolveOption)
            .Where(o => o is not null)
            .Select(o => o!)
            .ToList();

        var isImplicitHelpOptionAvailable = !optionSchemas.Any(o =>
            (o.ShortName is 'h')
            || string.Equals(o.Name, "help", StringComparison.OrdinalIgnoreCase)
        );

        if (isImplicitHelpOptionAvailable)
            optionSchemas.Add(CommandOptionSchema.ImplicitHelpOption);

        var isImplicitVersionOptionAvailable =
            string.IsNullOrWhiteSpace(name)
            && !optionSchemas.Any(o =>
                string.Equals(o.Name, "version", StringComparison.OrdinalIgnoreCase)
            );

        if (isImplicitVersionOptionAvailable)
            optionSchemas.Add(CommandOptionSchema.ImplicitVersionOption);

        return new CommandSchema(type, name, description, parameterSchemas, optionSchemas);
    }

    public static IReadOnlyList<CommandSchema> CompileMany(string sourceCode)
    {
        // Get default system namespaces
        var defaultSystemNamespaces = new[]
        {
            "System",
            "System.Collections",
            "System.Collections.Generic",
            "System.Linq",
            "System.Threading.Tasks",
            "System.Globalization",
        };

        // Get default CliFx namespaces
        var defaultCliFxNamespaces = typeof(ICommand)
            .Assembly.GetTypes()
            .Where(t => t.IsPublic)
            .Select(t => t.Namespace)
            .Distinct()
            .ToArray();

        // Append default imports to the source code
        var sourceCodeWithUsings =
            string.Join(Environment.NewLine, defaultSystemNamespaces.Select(n => $"using {n};"))
            + string.Join(Environment.NewLine, defaultCliFxNamespaces.Select(n => $"using {n};"))
            + Environment.NewLine
            + sourceCode;

        // Parse the source code
        var ast = SyntaxFactory.ParseSyntaxTree(
            SourceText.From(sourceCodeWithUsings),
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview)
        );

        // Compile the code to IL
        var compilation = CSharpCompilation.Create(
            "CliFxTests_DynamicAssembly_" + Guid.NewGuid(),
            [ast],
            Net100
                .References.All.Append(
                    MetadataReference.CreateFromFile(typeof(ICommand).Assembly.Location)
                )
                .Append(
                    MetadataReference.CreateFromFile(
                        typeof(DynamicCommandBuilder).Assembly.Location
                    )
                ),
            // DLL to avoid having to define the Main() method
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var compilationErrors = compilation
            .GetDiagnostics()
            .Where(d => d.Severity >= DiagnosticSeverity.Error)
            .ToArray();

        if (compilationErrors.Any())
        {
            throw new InvalidOperationException(
                $"""
                Failed to compile code.
                {string.Join(Environment.NewLine, compilationErrors.Select(e => e.ToString()))}
                """
            );
        }

        // Emit the code to an in-memory buffer
        using var buffer = new MemoryStream();
        var emit = compilation.Emit(buffer);

        var emitErrors = emit
            .Diagnostics.Where(d => d.Severity >= DiagnosticSeverity.Error)
            .ToArray();

        if (emitErrors.Any())
        {
            throw new InvalidOperationException(
                $"""
                Failed to emit code.
                {string.Join(Environment.NewLine, emitErrors.Select(e => e.ToString()))}
                """
            );
        }

        // Load the generated assembly
        var generatedAssembly = Assembly.Load(buffer.ToArray());

        // Return schemas for all defined commands
        var commandTypes = generatedAssembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(ICommand)) && !t.IsAbstract)
            .ToArray();

        if (commandTypes.Length <= 0)
        {
            throw new InvalidOperationException(
                "There are no command definitions in the provided source code."
            );
        }

        return commandTypes.Select(BuildSchema).ToArray();
    }

    public static CommandSchema Compile(string sourceCode)
    {
        var commandSchemas = CompileMany(sourceCode);
        if (commandSchemas.Count > 1)
        {
            throw new InvalidOperationException(
                "There are more than one command definitions in the provided source code."
            );
        }

        return commandSchemas.Single();
    }
}
