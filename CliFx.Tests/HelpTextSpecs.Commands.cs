using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Exceptions;

namespace CliFx.Tests
{
    public partial class HelpTextSpecs
    {
        [Command(Description = "DefaultCommand description.")]
        private class DefaultCommand : ICommand
        {
            [CommandOption("option-a", 'a', Description = "OptionA description.")]
            public string? OptionA { get; set; }

            [CommandOption("option-b", 'b', Description = "OptionB description.")]
            public string? OptionB { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
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

            public ValueTask ExecuteAsync(IConsole console) => default;
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

            public ValueTask ExecuteAsync(IConsole console) => default;
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

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command("cmd-with-req-opts")]
        private class RequiredOptionsCommand : ICommand
        {
            [CommandOption("option-f", 'f', IsRequired = true)]
            public string? OptionF { get; set; }

            [CommandOption("option-g", 'g', IsRequired = true)]
            public IEnumerable<int>? OptionG { get; set; }

            [CommandOption("option-h", 'h')]
            public string? OptionH { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command("cmd-with-enum-args")]
        private class EnumArgumentsCommand : ICommand
        {
            public enum TestEnum { Value1, Value2, Value3 };

            [CommandParameter(0, Name = "value", Description = "Enum parameter.")]
            public TestEnum ParamA { get; set; }

            [CommandOption("value", Description = "Enum option.", IsRequired = true)]
            public TestEnum OptionA { get; set; } = TestEnum.Value1;

            [CommandOption("nullable-value", Description = "Nullable enum option.")]
            public TestEnum? OptionB { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command("cmd-with-env-vars")]
        private class EnvironmentVariableCommand : ICommand
        {
            [CommandOption("option-a", 'a', IsRequired = true, EnvironmentVariableName = "ENV_OPT_A")]
            public string? OptionA { get; set; }

            [CommandOption("option-b", 'b', EnvironmentVariableName = "ENV_OPT_B")]
            public string? OptionB { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command("cmd-with-defaults")]
        private class DefaultArgumentsCommand : ICommand
        {
            [CommandOption(nameof(Object))]
            public object? Object { get; set; } = 42;

            [CommandOption(nameof(String))]
            public string? String { get; set; } = "foo";

            [CommandOption(nameof(Bool))]
            public bool Bool { get; set; } = true;

            [CommandOption(nameof(Char))]
            public char Char { get; set; } = 't';

            [CommandOption(nameof(Sbyte))]
            public sbyte Sbyte { get; set; } = -0b11;

            [CommandOption(nameof(Byte))]
            public byte Byte { get; set; } = 0b11;

            [CommandOption(nameof(Short))]
            public short Short { get; set; } = -1234;

            [CommandOption(nameof(Ushort))]
            public short Ushort { get; set; } = 1234;

            [CommandOption(nameof(Int))]
            public int Int { get; set; } = 1337;

            [CommandOption(nameof(Uint))]
            public uint Uint { get; set; } = 2345;

            [CommandOption(nameof(Long))]
            public long Long { get; set; } = -1234567;

            [CommandOption(nameof(Ulong))]
            public ulong Ulong { get; set; } = 12345678;

            [CommandOption(nameof(Float))]
            public float Float { get; set; } = 123.4567F;

            [CommandOption(nameof(Double))]
            public double Double { get; set; } = 420.1337;

            [CommandOption(nameof(Decimal))]
            public decimal Decimal { get; set; } = 1337.420M;

            [CommandOption(nameof(DateTime))]
            public DateTime DateTime { get; set; } = DateTime.Parse("Apr 20, 2020", CultureInfo.InvariantCulture);

            [CommandOption(nameof(DateTimeOffset))]
            public DateTimeOffset DateTimeOffset { get; set; } = DateTimeOffset.Parse("05/01/2008 +1:00", CultureInfo.InvariantCulture);

            [CommandOption(nameof(TimeSpan))]
            public TimeSpan TimeSpan { get; set; } = TimeSpan.FromMinutes(123);

            public enum TestEnum { Value1, Value2, Value3 };

            [CommandOption(nameof(CustomEnum))]
            public TestEnum CustomEnum { get; set; } = TestEnum.Value2;

            [CommandOption(nameof(IntNullable))]
            public int? IntNullable { get; set; } = 1337;

            [CommandOption(nameof(CustomEnumNullable))]
            public TestEnum? CustomEnumNullable { get; set; } = TestEnum.Value2;

            [CommandOption(nameof(TimeSpanNullable))]
            public TimeSpan? TimeSpanNullable { get; set; } = TimeSpan.FromMinutes(234);

            [CommandOption(nameof(ObjectArray))]
            public object[]? ObjectArray { get; set; } = new object[] { "123", 4, 3.14 };

            [CommandOption(nameof(StringArray))]
            public string[]? StringArray { get; set; } = new[] { "foo", "bar", "baz" };

            [CommandOption(nameof(IntArray))]
            public int[]? IntArray { get; set; } = new[] { 1, 2, 3 };

            [CommandOption(nameof(CustomEnumArray))]
            public TestEnum[]? CustomEnumArray { get; set; } = new[] { TestEnum.Value1, TestEnum.Value3 };

            [CommandOption(nameof(IntNullableArray))]
            public int?[]? IntNullableArray { get; set; } = new int?[] { 2, 3, 4, null, 5 };

            [CommandOption(nameof(EnumerableNullable))]
            public IEnumerable? EnumerableNullable { get; set; } = Enumerable.Repeat("foo", 3);

            [CommandOption(nameof(StringEnumerable))]
            public IEnumerable<string>? StringEnumerable { get; set; } = Enumerable.Repeat("bar", 3);

            [CommandOption(nameof(StringReadOnlyList))]
            public IReadOnlyList<string>? StringReadOnlyList { get; set; } = new List<string>() { "foo", "bar", "baz" }.AsReadOnly();

            [CommandOption(nameof(StringList))]
            public List<string>? StringList { get; set; } = new List<string>() { "foo", "bar", "baz" };

            [CommandOption(nameof(StringHashSet))]
            public HashSet<string>? StringHashSet { get; set; } = new HashSet<string>() { "foo", "bar", "baz" };

            public ValueTask ExecuteAsync(IConsole console) => default;
        }
    }
}