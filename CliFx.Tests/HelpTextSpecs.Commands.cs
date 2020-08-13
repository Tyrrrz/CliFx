namespace CliFx.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CliFx.Attributes;

    public partial class HelpTextSpecs
    {
        [Command(Description = "DefaultCommand description.")]
        private class DefaultCommand : ICommand
        {
            [CommandOption("option-a", 'a', Description = "OptionA description.")]
            public string? OptionA { get; set; }

            [CommandOption("option-b", 'b', Description = "OptionB description.")]
            public string? OptionB { get; set; }

            public ValueTask ExecuteAsync(IConsole console)
            {
                return default;
            }
        }

        [Command("cmd", Description = "NamedCommand description.")]
        private class NamedCommand : ICommand
        {
            [CommandParameter(0, Name = "param-a", Description = "ParameterA description.")]
            public string? ParameterA { get; set; }

            [CommandOption("option-c", 'c', Description = "OptionC description.")]
            public string? OptionC { get; set; }

            [CommandOption("option-d", 'd', Description = "OptionD description.")]
            public string? OptionD { get; set; }

            public ValueTask ExecuteAsync(IConsole console)
            {
                return default;
            }
        }

        [Command("cmd sub", Description = "NamedSubCommand description.")]
        private class NamedSubCommand : ICommand
        {
            [CommandParameter(0, Name = "param-b", Description = "ParameterB description.")]
            public string? ParameterB { get; set; }

            [CommandParameter(1, Name = "param-c", Description = "ParameterC description.")]
            public string? ParameterC { get; set; }

            [CommandOption("option-e", 'e', Description = "OptionE description.")]
            public string? OptionE { get; set; }

            public ValueTask ExecuteAsync(IConsole console)
            {
                return default;
            }
        }

        [Command("cmd-with-params")]
        private class ParametersCommand : ICommand
        {
            [CommandParameter(0, Name = "first")]
            public string? ParameterA { get; set; }

            [CommandParameter(10)]
            public int? ParameterB { get; set; }

            [CommandParameter(20, Description = "A list of numbers", Name = "third list")]
            public IEnumerable<int>? ParameterC { get; set; }

            [CommandOption("option", 'o')]
            public string? Option { get; set; }

            public ValueTask ExecuteAsync(IConsole console)
            {
                return default;
            }
        }

        [Command("cmd-with-req-opts")]
        private class RequiredOptionsCommand : ICommand
        {
            [CommandOption("option-a", 'a', IsRequired = true)]
            public string? OptionA { get; set; }

            [CommandOption("option-b", 'b', IsRequired = true)]
            public IEnumerable<int>? OptionB { get; set; }

            [CommandOption("option-c", 'c')]
            public string? OptionC { get; set; }

            public ValueTask ExecuteAsync(IConsole console)
            {
                return default;
            }
        }

        [Command("cmd-with-enum-args")]
        private class EnumArgumentsCommand : ICommand
        {
            public enum CustomEnum { Value1, Value2, Value3 };

            [CommandParameter(0, Name = "value", Description = "Enum parameter.")]
            public CustomEnum ParamA { get; set; }

            [CommandOption("value", Description = "Enum option.", IsRequired = true)]
            public CustomEnum OptionA { get; set; } = CustomEnum.Value1;

            [CommandOption("nullable-value", Description = "Nullable enum option.")]
            public CustomEnum? OptionB { get; set; }

            public ValueTask ExecuteAsync(IConsole console)
            {
                return default;
            }
        }

        [Command("cmd-with-env-vars")]
        private class EnvironmentVariableCommand : ICommand
        {
            [CommandOption("option-a", 'a', IsRequired = true, EnvironmentVariableName = "ENV_OPT_A")]
            public string? OptionA { get; set; }

            [CommandOption("option-b", 'b', EnvironmentVariableName = "ENV_OPT_B")]
            public string? OptionB { get; set; }

            public ValueTask ExecuteAsync(IConsole console)
            {
                return default;
            }
        }

        [Command("cmd-with-defaults")]
        private class ArgumentsWithDefaultValuesCommand : ICommand
        {
            public enum CustomEnum { Value1, Value2, Value3 };

            [CommandOption(nameof(Object))]
            public object? Object { get; set; } = 42;

            [CommandOption(nameof(String))]
            public string? String { get; set; } = "foo";

            [CommandOption(nameof(EmptyString))]
            public string EmptyString { get; set; } = "";

            [CommandOption(nameof(Bool))]
            public bool Bool { get; set; } = true;

            [CommandOption(nameof(Char))]
            public char Char { get; set; } = 't';

            [CommandOption(nameof(Int))]
            public int Int { get; set; } = 1337;

            [CommandOption(nameof(TimeSpan))]
            public TimeSpan TimeSpan { get; set; } = TimeSpan.FromMinutes(123);

            [CommandOption(nameof(Enum))]
            public CustomEnum Enum { get; set; } = CustomEnum.Value2;

            [CommandOption(nameof(IntNullable))]
            public int? IntNullable { get; set; } = 1337;

            [CommandOption(nameof(StringArray))]
            public string[]? StringArray { get; set; } = { "foo", "bar", "baz" };

            [CommandOption(nameof(IntArray))]
            public int[]? IntArray { get; set; } = { 1, 2, 3 };

            public ValueTask ExecuteAsync(IConsole console)
            {
                return default;
            }
        }
    }
}