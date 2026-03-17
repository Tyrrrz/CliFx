using System;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.SemanticModel;

/// <summary>
/// Resolves all well-known CliFx types from the current compilation.
/// Each type is looked up exactly once (by its metadata name), so renaming a CliFx type
/// only requires updating the single metadata name string here.
/// </summary>
internal sealed class CliFxReferences(Compilation compilation)
{
    // ── Roslyn type-check symbols ────────────────────────────────────────────
    public TypeDescriptor ICommand { get; } = Resolve(compilation, "CliFx.ICommand");

    public TypeDescriptor CommandAttribute { get; } =
        Resolve(compilation, CommandAttributeMetadataName);

    public TypeDescriptor CommandParameterAttribute { get; } =
        Resolve(compilation, "CliFx.Binding.CommandParameterAttribute");

    public TypeDescriptor CommandOptionAttribute { get; } =
        Resolve(compilation, "CliFx.Binding.CommandOptionAttribute");

    /// <summary>Open generic base class — used to check whether a user type is a custom converter.</summary>
    public TypeDescriptor InputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.InputConverter`1");

    /// <summary>Open generic base class — used to check whether a user type is a custom validator.</summary>
    public TypeDescriptor InputValidator { get; } =
        Resolve(compilation, "CliFx.Activation.InputValidator`1");

    // ── Code-emission symbols ────────────────────────────────────────────────
    public TypeDescriptor CliApplicationBuilder { get; } =
        Resolve(compilation, "CliFx.CliApplicationBuilder");
    public TypeDescriptor ICommandWithHelpOption { get; } =
        Resolve(compilation, "CliFx.ICommandWithHelpOption");
    public TypeDescriptor ICommandWithVersionOption { get; } =
        Resolve(compilation, "CliFx.ICommandWithVersionOption");

    public TypeDescriptor CommandDescriptor { get; } =
        Resolve(compilation, "CliFx.Binding.CommandDescriptor");
    public TypeDescriptor CommandInputDescriptor { get; } =
        Resolve(compilation, "CliFx.Binding.CommandInputDescriptor");
    public TypeDescriptor CommandParameterDescriptor { get; } =
        Resolve(compilation, "CliFx.Binding.CommandParameterDescriptor");
    public TypeDescriptor CommandOptionDescriptor { get; } =
        Resolve(compilation, "CliFx.Binding.CommandOptionDescriptor");
    public TypeDescriptor PropertyDescriptor { get; } =
        Resolve(compilation, "CliFx.Binding.PropertyDescriptor");
    public TypeDescriptor StringScalarInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.StringScalarInputConverter");
    public TypeDescriptor ObjectScalarInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.ObjectScalarInputConverter");
    public TypeDescriptor BoolScalarInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.BoolScalarInputConverter");
    public TypeDescriptor DateTimeOffsetScalarInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.DateTimeOffsetScalarInputConverter");
    public TypeDescriptor DateTimeScalarInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.DateTimeScalarInputConverter");
    public TypeDescriptor TimeSpanScalarInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.TimeSpanScalarInputConverter");

    public TypeDescriptor EnumScalarInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.EnumScalarInputConverter`1");
    public TypeDescriptor NullableScalarInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.NullableScalarInputConverter`1");
    public TypeDescriptor ConvertibleScalarInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.ConvertibleScalarInputConverter`1");
    public TypeDescriptor DelegateScalarInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.DelegateScalarInputConverter`1");
    public TypeDescriptor ArraySequenceInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.ArraySequenceInputConverter`1");
    public TypeDescriptor ArrayInitializableSequenceInputConverter { get; } =
        Resolve(compilation, "CliFx.Activation.ArrayInitializableSequenceInputConverter`2");

    // ────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Metadata name of the <c>[Command]</c> attribute.
    /// </summary>
    public const string CommandAttributeMetadataName = "CliFx.Binding.CommandAttribute";

    private static TypeDescriptor Resolve(Compilation compilation, string metadataName) =>
        new(
            compilation.GetTypeByMetadataName(metadataName)
                ?? throw new InvalidOperationException(
                    $"Could not resolve type '{metadataName}' in the current compilation."
                )
        );
}
