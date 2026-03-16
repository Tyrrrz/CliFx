using System.Collections.Generic;
using System.Linq;
using CliFx.Exceptions;
using CliFx.Utils.Extensions;

namespace CliFx.Infrastructure.Binding;

/// <summary>
/// Base type for scalar (single-value) converters.
/// </summary>
public abstract class ScalarBindingConverter<T> : BindingConverter<T>
{
    /// <inheritdoc />
    public override bool IsSequence => false;

    /// <summary>
    /// Parses the input value from the provided singular command-line argument.
    /// </summary>
    public abstract T Convert(string? rawValue);

    /// <inheritdoc />
    public sealed override T Convert(IReadOnlyList<string> rawValues) =>
        rawValues.Count <= 1
            ? Convert(rawValues.FirstOrDefault())
            : throw CliFxException.UserError(
                $"""
                Expected a single argument, but provided with multiple:
                {rawValues.Select(v => '<' + v + '>').JoinToString(" ")}
                """
            );
}
