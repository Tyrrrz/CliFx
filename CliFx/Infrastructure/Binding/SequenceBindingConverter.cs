namespace CliFx.Infrastructure.Binding;

/// <summary>
/// Base type for sequence (multi-value) converters.
/// </summary>
public abstract class SequenceBindingConverter<T> : BindingConverter<T>
{
    /// <inheritdoc />
    public override bool IsSequence => true;
}
