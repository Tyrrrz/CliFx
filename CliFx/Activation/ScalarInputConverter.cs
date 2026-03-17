using System.Collections.Generic;
using System.Linq;

namespace CliFx.Activation;

/// <summary>
/// Base type for scalar (single-value) converters.
/// </summary>
public abstract class ScalarInputConverter<T> : InputConverter<T>
{
    /// <inheritdoc />
    public override bool IsSequence => false;

    /// <summary>
    /// Converts the input value from the provided singular command-line argument.
    /// </summary>
    public abstract T Convert(string? rawValue);

    /// <inheritdoc />
    public sealed override T Convert(IReadOnlyList<string> rawValues) =>
        rawValues.Count <= 1
            ? Convert(rawValues.FirstOrDefault())
            : throw CliFxException.UserError(
                $"""
                Expected a single argument, but provided with multiple:
                {string.Join(" ", rawValues.Select(v => '<' + v + '>'))}
                """
            );
}
