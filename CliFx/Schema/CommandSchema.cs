using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CliFx.Exceptions;
using CliFx.Input;
using CliFx.Utils.Extensions;

namespace CliFx.Schema;

/// <summary>
/// Describes the schema of a command.
/// </summary>
public partial class CommandSchema(
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type type,
    string? name,
    string? description,
    IReadOnlyList<CommandParameterSchema> parameters,
    IReadOnlyList<CommandOptionSchema> options
)
{
    /// <summary>
    /// CLR type of the command.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type Type { get; } = type;

    /// <summary>
    /// Command name. Null or empty for the default command.
    /// </summary>
    public string? Name { get; } = name;

    /// <summary>
    /// Whether this is the default command (no name).
    /// </summary>
    public bool IsDefault => string.IsNullOrWhiteSpace(Name);

    /// <summary>
    /// Command description shown in help text.
    /// </summary>
    public string? Description { get; } = description;

    /// <summary>
    /// Parameters of the command.
    /// </summary>
    public IReadOnlyList<CommandParameterSchema> Parameters { get; } = parameters;

    /// <summary>
    /// Options of the command.
    /// </summary>
    public IReadOnlyList<CommandOptionSchema> Options { get; } = options;

    internal bool MatchesName(string? name) =>
        !string.IsNullOrWhiteSpace(Name)
            ? string.Equals(name, Name, StringComparison.OrdinalIgnoreCase)
            : string.IsNullOrWhiteSpace(name);

    internal IReadOnlyDictionary<CommandInputSchema, object?> GetValues(ICommand instance)
    {
        var result = new Dictionary<CommandInputSchema, object?>();

        foreach (var parameterSchema in Parameters)
        {
            var value = parameterSchema.Property.GetValue(instance);
            result[parameterSchema] = value;
        }

        foreach (var optionSchema in Options)
        {
            var value = optionSchema.Property.GetValue(instance);
            result[optionSchema] = value;
        }

        return result;
    }

    private void BindParameters(
        CommandInput input,
        IReadOnlyList<CommandParameterSchema> parameters,
        ICommand instance,
        bool throwOnUnrecognizedAndMissing = true
    )
    {
        var remainingParameterInputs = input.Parameters.ToList();
        var remainingRequiredParameters = parameters.Where(p => p.IsRequired).ToList();

        var position = 0;
        foreach (var parameter in parameters.OrderBy(p => p.Order))
        {
            if (position >= input.Parameters.Count)
                break;

            if (!parameter.IsSequence)
            {
                var parameterInput = input.Parameters[position];
                parameter.Bind([parameterInput.Value], instance);

                position++;
                remainingParameterInputs.Remove(parameterInput);
            }
            else
            {
                var parameterInputs = input.Parameters.Skip(position).ToArray();

                parameter.Bind(parameterInputs.Select(p => p.Value).ToArray(), instance);

                position += parameterInputs.Length;
                remainingParameterInputs.RemoveRange(parameterInputs);
            }

            remainingRequiredParameters.Remove(parameter);
        }

        if (throwOnUnrecognizedAndMissing)
        {
            if (remainingParameterInputs.Any())
            {
                throw CliFxException.UserError(
                    $"""
                    Unrecognized parameter(s):
                    {remainingParameterInputs.Select(p => p.GetFormattedIdentifier()).JoinToString(
                        " "
                    )}
                    """
                );
            }

            if (remainingRequiredParameters.Any())
            {
                throw CliFxException.UserError(
                    $"""
                     Missing required parameter(s):
                     {remainingRequiredParameters
                         .Select(p => p.GetFormattedIdentifier())
                         .JoinToString(" ")}
                     """
                );
            }
        }
    }

    private void BindOptions(
        CommandInput input,
        IReadOnlyList<CommandOptionSchema> options,
        ICommand instance,
        bool throwOnUnrecognizedAndMissing = true
    )
    {
        var remainingOptionInputs = input.Options.ToList();
        var remainingRequiredOptions = options.Where(o => o.IsRequired).ToList();

        foreach (var option in options)
        {
            var optionInputs = input
                .Options.Where(o => option.MatchesIdentifier(o.Identifier))
                .ToArray();

            var environmentVariableInput = input.EnvironmentVariables.FirstOrDefault(e =>
                option.MatchesEnvironmentVariable(e.Name)
            );

            if (optionInputs.Any())
            {
                var rawValues = optionInputs.SelectMany(o => o.Values).ToArray();

                option.Bind(rawValues, instance);

                if (rawValues.Any())
                    remainingRequiredOptions.Remove(option);
            }
            else if (environmentVariableInput is not null)
            {
                var rawValues = !option.IsSequence
                    ? [environmentVariableInput.Value]
                    : environmentVariableInput.SplitValues();

                option.Bind(rawValues, instance);

                if (rawValues.Any())
                    remainingRequiredOptions.Remove(option);
            }
            else
            {
                continue;
            }

            remainingOptionInputs.RemoveRange(optionInputs);
        }

        if (throwOnUnrecognizedAndMissing)
        {
            if (remainingOptionInputs.Any())
            {
                throw CliFxException.UserError(
                    $"""
                    Unrecognized option(s):
                    {remainingOptionInputs.Select(o => o.GetFormattedIdentifier()).JoinToString(
                        ", "
                    )}
                    """
                );
            }

            if (remainingRequiredOptions.Any())
            {
                throw CliFxException.UserError(
                    $"""
                     Missing required option(s):
                     {remainingRequiredOptions
                         .Select(o => o.GetFormattedIdentifier())
                         .JoinToString(", ")}
                     """
                );
            }
        }
    }

    internal void BindHelpAndVersionOptions(CommandInput input, ICommand instance)
    {
        var options = new List<CommandOptionSchema>(2);

        if (instance is IHasHelpOption)
        {
            var optionSchema = Options.FirstOrDefault(o =>
                string.Equals(
                    o.Property.Name,
                    nameof(IHasHelpOption.IsHelpRequested),
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (optionSchema is not null)
                options.Add(optionSchema);
        }

        if (instance is IHasVersionOption)
        {
            var optionSchema = Options.FirstOrDefault(o =>
                string.Equals(
                    o.Property.Name,
                    nameof(IHasVersionOption.IsVersionRequested),
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (optionSchema is not null)
                options.Add(optionSchema);
        }

        if (!options.Any())
            return;

        BindOptions(input, options, instance, false);
    }

    internal void Bind(CommandInput input, ICommand instance)
    {
        BindParameters(input, Parameters, instance);
        BindOptions(input, Options, instance);
    }
}

public partial class CommandSchema : IEquatable<CommandSchema>
{
    /// <inheritdoc />
    public bool Equals(CommandSchema? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return Type == other.Type;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;

        return Equals((CommandSchema)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() => Type.GetHashCode();

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(CommandSchema? left, CommandSchema? right) =>
        Equals(left, right);

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator !=(CommandSchema? left, CommandSchema? right) =>
        !Equals(left, right);
}

/// <inheritdoc cref="CommandSchema" />
/// <remarks>
/// Generic version used by source-generated code for static type references and AOT compatibility.
/// </remarks>
public class CommandSchema<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        TCommand
>(string? name, string? description, IReadOnlyList<CommandInputSchema> inputs)
    : CommandSchema(
        typeof(TCommand),
        name,
        description,
        inputs.OfType<CommandParameterSchema>().ToArray(),
        inputs.OfType<CommandOptionSchema>().ToArray()
    )
    where TCommand : ICommand;
