using System;

namespace CliFx.Schema;

internal static class CommandInputSchemaExtensions
{
    extension(CommandInputSchema schema)
    {
        public string GetKind() =>
            schema switch
            {
                CommandParameterSchema => "Parameter",
                CommandOptionSchema => "Option",
                _ => throw new InvalidOperationException(
                    $"Unknown input schema type: '{schema.GetType().Name}'."
                ),
            };

        public string GetFormattedIdentifier() =>
            schema switch
            {
                CommandParameterSchema parameter => parameter.IsSequence
                    ? $"<{parameter.Name}...>"
                    : $"<{parameter.Name}>",

                CommandOptionSchema option => option switch
                {
                    { Name: not null, ShortName: not null } =>
                        $"-{option.ShortName}|--{option.Name}",
                    { Name: not null } => $"--{option.Name}",
                    { ShortName: not null } => $"-{option.ShortName}",
                    _ => throw new InvalidOperationException(
                        "Option must have a name or short name."
                    ),
                },
                _ => throw new ArgumentOutOfRangeException(nameof(schema)),
            };
    }
}
