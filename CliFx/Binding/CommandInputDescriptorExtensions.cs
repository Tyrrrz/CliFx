using System;

namespace CliFx.Binding;

internal static class CommandInputDescriptorExtensions
{
    extension(CommandInputDescriptor descriptor)
    {
        public string GetKind() =>
            descriptor switch
            {
                CommandParameterDescriptor => "Parameter",
                CommandOptionDescriptor => "Option",
                _ => throw new InvalidOperationException(
                    $"Unknown input descriptor type: '{descriptor.GetType().Name}'."
                ),
            };

        public string GetFormattedIdentifier() =>
            descriptor switch
            {
                CommandParameterDescriptor parameter => parameter.Converter.IsSequence
                    ? $"<{parameter.Name}...>"
                    : $"<{parameter.Name}>",

                CommandOptionDescriptor option => option switch
                {
                    { Name: not null, ShortName: not null } =>
                        $"-{option.ShortName}|--{option.Name}",
                    { Name: not null } => $"--{option.Name}",
                    { ShortName: not null } => $"-{option.ShortName}",
                    _ => throw new InvalidOperationException(
                        "Option must have a name or short name."
                    ),
                },
                _ => throw new ArgumentOutOfRangeException(nameof(descriptor)),
            };
    }
}
