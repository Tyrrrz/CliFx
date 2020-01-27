using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Tests.TestCustomTypes;

namespace CliFx.Tests.TestCommands
{
    [Command]
    public class AllSupportedTypesCommand : ICommand
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

        [CommandOption(nameof(TestEnum))]
        public TestEnum TestEnum { get; set; }

        [CommandOption(nameof(IntNullable))]
        public int? IntNullable { get; set; }

        [CommandOption(nameof(TestEnumNullable))]
        public TestEnum? TestEnumNullable { get; set; }

        [CommandOption(nameof(TimeSpanNullable))]
        public TimeSpan? TimeSpanNullable { get; set; }

        [CommandOption(nameof(TestStringConstructable))]
        public TestStringConstructable? TestStringConstructable { get; set; }

        [CommandOption(nameof(TestStringParseable))]
        public TestStringParseable? TestStringParseable { get; set; }

        [CommandOption(nameof(TestStringParseableWithFormatProvider))]
        public TestStringParseableWithFormatProvider? TestStringParseableWithFormatProvider { get; set; }

        [CommandOption(nameof(ObjectArray))]
        public object[]? ObjectArray { get; set; }

        [CommandOption(nameof(StringArray))]
        public string[]? StringArray { get; set; }

        [CommandOption(nameof(IntArray))]
        public int[]? IntArray { get; set; }

        [CommandOption(nameof(TestEnumArray))]
        public TestEnum[]? TestEnumArray { get; set; }

        [CommandOption(nameof(IntNullableArray))]
        public int?[]? IntNullableArray { get; set; }

        [CommandOption(nameof(TestStringConstructableArray))]
        public TestStringConstructable[]? TestStringConstructableArray { get; set; }

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

        [CommandOption(nameof(NonConvertible))]
        public TestNonStringParseable? NonConvertible { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}