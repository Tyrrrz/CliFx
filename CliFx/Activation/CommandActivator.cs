using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Binding;
using CliFx.Exceptions;
using CliFx.Parsing;
using CliFx.Utils.Extensions;

namespace CliFx.Activation;

internal class CommandActivator(CommandDescriptor command, ICommand instance)
{
    private static void ActivateInput(
        CommandInputDescriptor input,
        ICommand instance,
        IReadOnlyList<string> rawValues
    )
    {
        var value = input.Converter.Convert(rawValues);

        var errors = input
            .Validators.Select(validator => validator.Validate(value))
            .WhereNotNull()
            .ToArray();

        if (errors.Any())
        {
            throw CliFxException.UserError(
                $"""
                {input.GetKind()} {input.GetFormattedIdentifier()} has been provided with an invalid value.
                Error(s):
                {string.Join(Environment.NewLine, errors.Select(e => "- " + e.Message))}
                """
            );
        }

        try
        {
            input.Property.SetValue(instance, value);
        }
        catch (Exception ex) when (ex is not CliFxException)
        {
            throw CliFxException.UserError(
                $"""
                {input.GetKind()} {input.GetFormattedIdentifier()} cannot be set from the provided argument(s):
                {string.Join(" ", rawValues.Select(v => '<' + v + '>'))}
                Error: {ex.Message}
                """,
                ex
            );
        }
    }

    private void ActivateParameters(
        IReadOnlyList<CommandParameterDescriptor> parameters,
        ParsedCommandLine commandLine,
        bool throwOnUnrecognizedAndMissing = true
    )
    {
        var remainingPositionalArguments = commandLine.PositionalArguments.ToList();
        var remainingRequiredParameters = parameters.Where(p => p.IsRequired).ToList();

        var position = 0;
        foreach (var parameter in parameters.OrderBy(p => p.Order))
        {
            if (position >= commandLine.PositionalArguments.Count)
                break;

            if (!parameter.Converter.IsSequence)
            {
                var positionalArgument = commandLine.PositionalArguments[position];
                ActivateInput(parameter, instance, [positionalArgument.Value]);

                position++;
                remainingPositionalArguments.Remove(positionalArgument);
            }
            else
            {
                var positionalArguments = commandLine.PositionalArguments.Skip(position).ToArray();

                ActivateInput(
                    parameter,
                    instance,
                    positionalArguments.Select(p => p.Value).ToArray()
                );

                position += positionalArguments.Length;
                remainingPositionalArguments.RemoveRange(positionalArguments);
            }

            remainingRequiredParameters.Remove(parameter);
        }

        if (throwOnUnrecognizedAndMissing)
        {
            if (remainingPositionalArguments.Any())
            {
                throw CliFxException.UserError(
                    $"""
                    Unrecognized parameter(s):
                    {string.Join(" ", remainingPositionalArguments.Select(p =>
                        p.GetFormattedIdentifier()
                    ))}
                    """
                );
            }

            if (remainingRequiredParameters.Any())
            {
                throw CliFxException.UserError(
                    $"""
                     Missing required parameter(s):
                     {string.Join(
                         " ",
                         remainingRequiredParameters.Select(p => p.GetFormattedIdentifier())
                     )}
                     """
                );
            }
        }
    }

    private void ActivateOptions(
        IReadOnlyList<CommandOptionDescriptor> options,
        ParsedCommandLine commandLine,
        bool throwOnUnrecognizedAndMissing = true
    )
    {
        var remainingParsedOptions = commandLine.Options.ToList();
        var remainingRequiredOptions = options.Where(o => o.IsRequired).ToList();

        foreach (var option in options)
        {
            var parsedOptions = commandLine
                .Options.Where(o => option.MatchesIdentifier(o.Identifier))
                .ToArray();

            var parsedEnvironmentVariable = commandLine.EnvironmentVariables.FirstOrDefault(e =>
                option.MatchesEnvironmentVariable(e.Name)
            );

            if (parsedOptions.Any())
            {
                var rawValues = parsedOptions.SelectMany(o => o.Values).ToArray();

                ActivateInput(option, instance, rawValues);

                if (rawValues.Any())
                    remainingRequiredOptions.Remove(option);
            }
            else if (parsedEnvironmentVariable is not null)
            {
                var rawValues = !option.Converter.IsSequence
                    ? [parsedEnvironmentVariable.Value]
                    : parsedEnvironmentVariable.SplitValues();

                ActivateInput(option, instance, rawValues);

                if (rawValues.Any())
                    remainingRequiredOptions.Remove(option);
            }
            else
            {
                continue;
            }

            remainingParsedOptions.RemoveRange(parsedOptions);
        }

        if (throwOnUnrecognizedAndMissing)
        {
            if (remainingParsedOptions.Any())
            {
                throw CliFxException.UserError(
                    $"""
                    Unrecognized option(s):
                    {string.Join(
                        ", ",
                        remainingParsedOptions.Select(o => o.GetFormattedIdentifier())
                    )}
                    """
                );
            }

            if (remainingRequiredOptions.Any())
            {
                throw CliFxException.UserError(
                    $"""
                     Missing required option(s):
                     {string.Join(
                         ", ",
                         remainingRequiredOptions.Select(o => o.GetFormattedIdentifier())
                     )}
                     """
                );
            }
        }
    }

    public void ActivateHelpAndVersionOptions(ParsedCommandLine commandLine)
    {
        var options = new List<CommandOptionDescriptor>(2);

        if (instance is ICommandWithHelpOption)
        {
            var optionDescriptor = command.Options.FirstOrDefault(o =>
                string.Equals(
                    o.Property.Name,
                    nameof(ICommandWithHelpOption.IsHelpRequested),
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (optionDescriptor is not null)
                options.Add(optionDescriptor);
        }

        if (instance is ICommandWithVersionOption)
        {
            var optionDescriptor = command.Options.FirstOrDefault(o =>
                string.Equals(
                    o.Property.Name,
                    nameof(ICommandWithVersionOption.IsVersionRequested),
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (optionDescriptor is not null)
                options.Add(optionDescriptor);
        }

        if (!options.Any())
            return;

        ActivateOptions(options, commandLine, false);
    }

    public void Activate(ParsedCommandLine commandLine)
    {
        ActivateParameters(command.Parameters, commandLine);
        ActivateOptions(command.Options, commandLine);
    }
}
