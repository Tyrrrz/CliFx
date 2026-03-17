namespace CliFx.Activation;

/// <summary>
/// Base type for sequence (multi-value) converters.
/// </summary>
public abstract class SequenceInputConverter<T> : InputConverter<T>
{
    /// <inheritdoc />
    public override bool IsSequence => true;
}
