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
    /// <summary>Checked via <c>Symbol</c> equality to verify a class implements ICommand.</summary>
    public TypeDescriptor ICommand { get; } = Resolve(compilation, "CliFx.ICommand");

    /// <summary>Checked via <c>Symbol</c> equality in attribute matching inside TryBuildCommandDescriptor.</summary>
    public TypeDescriptor CommandAttribute { get; } =
        Resolve(compilation, CommandAttributeMetadataName);

    /// <summary>Checked via <c>Symbol</c> equality in attribute matching inside TryBuildCommandDescriptor.</summary>
    public TypeDescriptor CommandParameterAttribute { get; } =
        Resolve(compilation, "CliFx.CommandParameterAttribute");

    /// <summary>Checked via <c>Symbol</c> equality in attribute matching inside TryBuildCommandDescriptor.</summary>
    public TypeDescriptor CommandOptionAttribute { get; } =
        Resolve(compilation, "CliFx.CommandOptionAttribute");

    /// <summary>Open generic base class — used to check whether a user type is a custom converter.</summary>
    public TypeDescriptor BindingConverter { get; } =
        Resolve(compilation, "CliFx.Infrastructure.Binding.BindingConverter`1");

    /// <summary>Open generic base class — used to check whether a user type is a custom validator.</summary>
    public TypeDescriptor BindingValidator { get; } =
        Resolve(compilation, "CliFx.Infrastructure.Binding.BindingValidator`1");

    // ── Code-emission symbols ────────────────────────────────────────────────
    // Non-generic types: use ToString() directly in interpolations.
    // Open-generic types: use .GlobalBase when emitting a concrete instantiation.
    public TypeDescriptor CliApplicationBuilder { get; } =
        Resolve(compilation, "CliFx.CliApplicationBuilder");
    public TypeDescriptor IHasHelpOption { get; } =
        Resolve(compilation, "CliFx.Schema.IHasHelpOption");
    public TypeDescriptor IHasVersionOption { get; } =
        Resolve(compilation, "CliFx.Schema.IHasVersionOption");

    // Non-generic base versions — ToString() gives the bare global:: name used in emission.
    public TypeDescriptor CommandSchema { get; } =
        Resolve(compilation, "CliFx.Schema.CommandSchema");
    public TypeDescriptor CommandInputSchema { get; } =
        Resolve(compilation, "CliFx.Schema.CommandInputSchema");
    public TypeDescriptor CommandParameterSchema { get; } =
        Resolve(compilation, "CliFx.Schema.CommandParameterSchema");
    public TypeDescriptor CommandOptionSchema { get; } =
        Resolve(compilation, "CliFx.Schema.CommandOptionSchema");
    public TypeDescriptor PropertyBinding { get; } =
        Resolve(compilation, "CliFx.Schema.PropertyBinding");
    public TypeDescriptor StringScalarBindingConverter { get; } =
        Resolve(compilation, "CliFx.Infrastructure.Binding.StringScalarBindingConverter");
    public TypeDescriptor ObjectScalarBindingConverter { get; } =
        Resolve(compilation, "CliFx.Infrastructure.Binding.ObjectScalarBindingConverter");
    public TypeDescriptor BoolScalarBindingConverter { get; } =
        Resolve(compilation, "CliFx.Infrastructure.Binding.BoolScalarBindingConverter");
    public TypeDescriptor DateTimeOffsetScalarBindingConverter { get; } =
        Resolve(compilation, "CliFx.Infrastructure.Binding.DateTimeOffsetScalarBindingConverter");
    public TypeDescriptor DateTimeScalarBindingConverter { get; } =
        Resolve(compilation, "CliFx.Infrastructure.Binding.DateTimeScalarBindingConverter");
    public TypeDescriptor TimeSpanScalarBindingConverter { get; } =
        Resolve(compilation, "CliFx.Infrastructure.Binding.TimeSpanScalarBindingConverter");

    // Open-generic types — use .GlobalBase when appending concrete type arguments.
    public TypeDescriptor EnumScalarBindingConverter { get; } =
        Resolve(compilation, "CliFx.Infrastructure.Binding.EnumScalarBindingConverter`1");
    public TypeDescriptor NullableScalarBindingConverter { get; } =
        Resolve(compilation, "CliFx.Infrastructure.Binding.NullableScalarBindingConverter`1");
    public TypeDescriptor ConvertibleScalarBindingConverter { get; } =
        Resolve(compilation, "CliFx.Infrastructure.Binding.ConvertibleScalarBindingConverter`1");
    public TypeDescriptor DelegateScalarBindingConverter { get; } =
        Resolve(compilation, "CliFx.Infrastructure.Binding.DelegateScalarBindingConverter`1");
    public TypeDescriptor ArraySequenceBindingConverter { get; } =
        Resolve(compilation, "CliFx.Infrastructure.Binding.ArraySequenceBindingConverter`1");
    public TypeDescriptor ArrayInitializableSequenceBindingConverter { get; } =
        Resolve(
            compilation,
            "CliFx.Infrastructure.Binding.ArrayInitializableSequenceBindingConverter`2"
        );

    // ────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Metadata name of the <c>[Command]</c> attribute.
    /// Exposed as a constant so it can be passed to
    /// <c>ForAttributeWithMetadataName</c> at generator-initialize time,
    /// before a <see cref="Compilation"/> is available.
    /// </summary>
    public const string CommandAttributeMetadataName = "CliFx.CommandAttribute";

    private static TypeDescriptor Resolve(Compilation compilation, string metadataName) =>
        new(
            compilation.GetTypeByMetadataName(metadataName)
                ?? throw new InvalidOperationException(
                    $"Could not resolve type '{metadataName}' in the current compilation."
                )
        );
}
