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
    public TypeDescriptor ICommand { get; } =
        new(compilation.GetTypeByMetadataName("CliFx.ICommand")!);

    /// <summary>Checked via <c>Symbol</c> equality in attribute matching inside TryBuildCommandDescriptor.</summary>
    public TypeDescriptor CommandAttribute { get; } =
        new(compilation.GetTypeByMetadataName(CommandAttributeMetadataName)!);

    /// <summary>Checked via <c>Symbol</c> equality in attribute matching inside TryBuildCommandDescriptor.</summary>
    public TypeDescriptor CommandParameterAttribute { get; } =
        new(compilation.GetTypeByMetadataName("CliFx.Attributes.CommandParameterAttribute")!);

    /// <summary>Checked via <c>Symbol</c> equality in attribute matching inside TryBuildCommandDescriptor.</summary>
    public TypeDescriptor CommandOptionAttribute { get; } =
        new(compilation.GetTypeByMetadataName("CliFx.Attributes.CommandOptionAttribute")!);

    /// <summary>Open generic base class — used to check whether a user type is a custom converter.</summary>
    public TypeDescriptor BindingConverter { get; } =
        new(compilation.GetTypeByMetadataName("CliFx.Infrastructure.Binding.BindingConverter`1")!);

    /// <summary>Open generic base class — used to check whether a user type is a custom validator.</summary>
    public TypeDescriptor BindingValidator { get; } =
        new(compilation.GetTypeByMetadataName("CliFx.Infrastructure.Binding.BindingValidator`1")!);

    // ── Code-emission symbols ────────────────────────────────────────────────
    // Non-generic types: use ToString() directly in interpolations.
    // Open-generic types: use .GlobalBase when emitting a concrete instantiation.
    public TypeDescriptor CliApplicationBuilder { get; } =
        new(compilation.GetTypeByMetadataName("CliFx.CliApplicationBuilder")!);
    public TypeDescriptor IHasHelpOption { get; } =
        new(compilation.GetTypeByMetadataName("CliFx.Schema.IHasHelpOption")!);
    public TypeDescriptor IHasVersionOption { get; } =
        new(compilation.GetTypeByMetadataName("CliFx.Schema.IHasVersionOption")!);

    // Non-generic base versions — ToString() gives the bare global:: name used in emission.
    public TypeDescriptor CommandSchema { get; } =
        new(compilation.GetTypeByMetadataName("CliFx.Schema.CommandSchema")!);
    public TypeDescriptor CommandInputSchema { get; } =
        new(compilation.GetTypeByMetadataName("CliFx.Schema.CommandInputSchema")!);
    public TypeDescriptor CommandParameterSchema { get; } =
        new(compilation.GetTypeByMetadataName("CliFx.Schema.CommandParameterSchema")!);
    public TypeDescriptor CommandOptionSchema { get; } =
        new(compilation.GetTypeByMetadataName("CliFx.Schema.CommandOptionSchema")!);
    public TypeDescriptor PropertyBinding { get; } =
        new(compilation.GetTypeByMetadataName("CliFx.Schema.PropertyBinding")!);
    public TypeDescriptor IBindingValidator { get; } =
        new(compilation.GetTypeByMetadataName("CliFx.Infrastructure.Binding.IBindingValidator")!);
    public TypeDescriptor BoolBindingConverter { get; } =
        new(
            compilation.GetTypeByMetadataName("CliFx.Infrastructure.Binding.BoolBindingConverter")!
        );
    public TypeDescriptor DateTimeOffsetBindingConverter { get; } =
        new(
            compilation.GetTypeByMetadataName(
                "CliFx.Infrastructure.Binding.DateTimeOffsetBindingConverter"
            )!
        );
    public TypeDescriptor TimeSpanBindingConverter { get; } =
        new(
            compilation.GetTypeByMetadataName(
                "CliFx.Infrastructure.Binding.TimeSpanBindingConverter"
            )!
        );

    // Open-generic types — use .GlobalBase when appending concrete type arguments.
    public TypeDescriptor EnumBindingConverter { get; } =
        new(
            compilation.GetTypeByMetadataName(
                "CliFx.Infrastructure.Binding.EnumBindingConverter`1"
            )!
        );
    public TypeDescriptor NullableBindingConverter { get; } =
        new(
            compilation.GetTypeByMetadataName(
                "CliFx.Infrastructure.Binding.NullableBindingConverter`1"
            )!
        );
    public TypeDescriptor ConvertibleBindingConverter { get; } =
        new(
            compilation.GetTypeByMetadataName(
                "CliFx.Infrastructure.Binding.ConvertibleBindingConverter`1"
            )!
        );
    public TypeDescriptor DelegateBindingConverter { get; } =
        new(
            compilation.GetTypeByMetadataName(
                "CliFx.Infrastructure.Binding.DelegateBindingConverter`1"
            )!
        );
    public TypeDescriptor ArraySequenceBindingConverter { get; } =
        new(
            compilation.GetTypeByMetadataName(
                "CliFx.Infrastructure.Binding.ArraySequenceBindingConverter`1"
            )!
        );
    public TypeDescriptor ArrayInitializableSequenceBindingConverter { get; } =
        new(
            compilation.GetTypeByMetadataName(
                "CliFx.Infrastructure.Binding.ArrayInitializableSequenceBindingConverter`2"
            )!
        );

    // ────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Metadata name of the <c>[Command]</c> attribute.
    /// Exposed as a constant so it can be passed to
    /// <c>ForAttributeWithMetadataName</c> at generator-initialize time,
    /// before a <see cref="Compilation"/> is available.
    /// </summary>
    public const string CommandAttributeMetadataName = "CliFx.Attributes.CommandAttribute";

    public static CliFxReferences From(Compilation compilation) => new(compilation);
}
