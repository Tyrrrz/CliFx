using System;
using CliFx.Generators.Binding;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators;

internal partial class KnownSymbols(Compilation compilation)
{
    public TypeSymbol CliApplicationBuilder { get; } =
        Resolve(compilation, "CliFx.CliApplicationBuilder");

    public TypeSymbol ICommand { get; } = Resolve(compilation, "CliFx.ICommand");

    public TypeSymbol ICommandWithHelpOption { get; } =
        Resolve(compilation, "CliFx.ICommandWithHelpOption");

    public TypeSymbol ICommandWithVersionOption { get; } =
        Resolve(compilation, "CliFx.ICommandWithVersionOption");

    // Exposed separately because this needs to be used during generator initialization,
    // before compilation is available.
    public const string CommandAttributeMetadataName = "CliFx.Binding.CommandAttribute";

    public TypeSymbol CommandAttribute { get; } =
        Resolve(compilation, CommandAttributeMetadataName);

    public TypeSymbol CommandParameterAttribute { get; } =
        Resolve(compilation, "CliFx.Binding.CommandParameterAttribute");

    public TypeSymbol CommandOptionAttribute { get; } =
        Resolve(compilation, "CliFx.Binding.CommandOptionAttribute");

    public TypeSymbol CommandDescriptor { get; } =
        Resolve(compilation, "CliFx.Binding.CommandDescriptor");
    public TypeSymbol CommandInputDescriptor { get; } =
        Resolve(compilation, "CliFx.Binding.CommandInputDescriptor");
    public TypeSymbol CommandParameterDescriptor { get; } =
        Resolve(compilation, "CliFx.Binding.CommandParameterDescriptor");
    public TypeSymbol CommandOptionDescriptor { get; } =
        Resolve(compilation, "CliFx.Binding.CommandOptionDescriptor");
    public TypeSymbol PropertyDescriptor { get; } =
        Resolve(compilation, "CliFx.Binding.PropertyDescriptor");

    public TypeSymbol InputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.InputConverter`1");
    public TypeSymbol InputValidator { get; } =
        Resolve(compilation, "CliFx.Activation.InputValidator`1");
    public TypeSymbol StringScalarInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.StringScalarInputConverter");
    public TypeSymbol ObjectScalarInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.ObjectScalarInputConverter");
    public TypeSymbol BoolScalarInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.BoolScalarInputConverter");
    public TypeSymbol DateTimeOffsetScalarInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.DateTimeOffsetScalarInputConverter");
    public TypeSymbol DateTimeScalarInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.DateTimeScalarInputConverter");
    public TypeSymbol TimeSpanScalarInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.TimeSpanScalarInputConverter");
    public TypeSymbol EnumScalarInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.EnumScalarInputConverter`1");
    public TypeSymbol NullableScalarInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.NullableScalarInputConverter`1");
    public TypeSymbol ConvertibleScalarInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.ConvertibleScalarInputConverter`1");
    public TypeSymbol DelegateScalarInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.DelegateScalarInputConverter`1");
    public TypeSymbol ArraySequenceInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.ArraySequenceInputConverter`1");
    public TypeSymbol ArrayInitializableSequenceInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.ArrayInitializableSequenceInputConverter`2");
}

internal partial class KnownSymbols
{
    private static TypeSymbol Resolve(Compilation compilation, string fullyQualifiedName) =>
        new(
            compilation.GetTypeByMetadataName(fullyQualifiedName)
                ?? throw new InvalidOperationException(
                    $"Could not resolve type '{fullyQualifiedName}' in the current compilation."
                )
        );
}
