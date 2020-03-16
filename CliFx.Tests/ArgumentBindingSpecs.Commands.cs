using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests
{
    public partial class ArgumentBindingSpecs
    {
        [Command]
        private class AllSupportedTypesCommand : ICommand
        {
            [CommandOption(nameof(Object))]
            public object? Object { get; set; } = 42;

            [CommandOption(nameof(String))]
            public string? String { get; set; } = "foo bar";

            [CommandOption(nameof(Bool))]
            public bool Bool { get; set; }

            [CommandOption(nameof(Char))]
            public char Char { get; set; }

            [CommandOption(nameof(Sbyte))]
            public sbyte Sbyte { get; set; }

            [CommandOption(nameof(Byte))]
            public byte Byte { get; set; }

            [CommandOption(nameof(Short))]
            public short Short { get; set; }

            [CommandOption(nameof(Ushort))]
            public ushort Ushort { get; set; }

            [CommandOption(nameof(Int))]
            public int Int { get; set; }

            [CommandOption(nameof(Uint))]
            public uint Uint { get; set; }

            [CommandOption(nameof(Long))]
            public long Long { get; set; }

            [CommandOption(nameof(Ulong))]
            public ulong Ulong { get; set; }

            [CommandOption(nameof(Float))]
            public float Float { get; set; }

            [CommandOption(nameof(Double))]
            public double Double { get; set; }

            [CommandOption(nameof(Decimal))]
            public decimal Decimal { get; set; }

            [CommandOption(nameof(DateTime))]
            public DateTime DateTime { get; set; }

            [CommandOption(nameof(DateTimeOffset))]
            public DateTimeOffset DateTimeOffset { get; set; }

            [CommandOption(nameof(TimeSpan))]
            public TimeSpan TimeSpan { get; set; }

            [CommandOption(nameof(CustomEnum))]
            public CustomEnum CustomEnum { get; set; }

            [CommandOption(nameof(IntNullable))]
            public int? IntNullable { get; set; }

            [CommandOption(nameof(CustomEnumNullable))]
            public CustomEnum? CustomEnumNullable { get; set; }

            [CommandOption(nameof(TimeSpanNullable))]
            public TimeSpan? TimeSpanNullable { get; set; }

            [CommandOption(nameof(TestStringConstructable))]
            public StringConstructable? TestStringConstructable { get; set; }

            [CommandOption(nameof(TestStringParseable))]
            public StringParseable? TestStringParseable { get; set; }

            [CommandOption(nameof(TestStringParseableWithFormatProvider))]
            public StringParseableWithFormatProvider? TestStringParseableWithFormatProvider { get; set; }

            [CommandOption(nameof(ObjectArray))]
            public object[]? ObjectArray { get; set; }

            [CommandOption(nameof(StringArray))]
            public string[]? StringArray { get; set; }

            [CommandOption(nameof(IntArray))]
            public int[]? IntArray { get; set; }

            [CommandOption(nameof(CustomEnumArray))]
            public CustomEnum[]? CustomEnumArray { get; set; }

            [CommandOption(nameof(IntNullableArray))]
            public int?[]? IntNullableArray { get; set; }

            [CommandOption(nameof(TestStringConstructableArray))]
            public StringConstructable[]? TestStringConstructableArray { get; set; }

            [CommandOption(nameof(Enumerable))]
            public IEnumerable? Enumerable { get; set; }

            [CommandOption(nameof(StringEnumerable))]
            public IEnumerable<string>? StringEnumerable { get; set; }

            [CommandOption(nameof(StringReadOnlyList))]
            public IReadOnlyList<string>? StringReadOnlyList { get; set; }

            [CommandOption(nameof(StringList))]
            public List<string>? StringList { get; set; }

            [CommandOption(nameof(StringHashSet))]
            public HashSet<string>? StringHashSet { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command]
        private class RequiredOptionCommand : ICommand
        {
            [CommandOption(nameof(OptionA))]
            public string? OptionA { get; set; }

            [CommandOption(nameof(OptionB), IsRequired = true)]
            public string? OptionB { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command]
        private class ParametersCommand : ICommand
        {
            [CommandParameter(0)]
            public string? ParameterA { get; set; }

            [CommandParameter(1)]
            public string? ParameterB { get; set; }

            [CommandParameter(2)]
            public IReadOnlyList<string>? ParameterC { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command]
        private class UnsupportedPropertyTypeCommand : ICommand
        {
            [CommandOption(nameof(Option))]
            public DummyType? Option { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command]
        private class UnsupportedEnumerablePropertyTypeCommand : ICommand
        {
            [CommandOption(nameof(Option))]
            public CustomEnumerable<string>? Option { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command]
        private class NoParameterCommand : ICommand
        {
            [CommandOption(nameof(OptionA))]
            public string? OptionA { get; set; }

            [CommandOption(nameof(OptionB))]
            public string? OptionB { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }
    }
}