using CliFx.Attributes;
using CliFx.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CliFx.Tests.Dummy.Commands
{
    /// <summary>
    /// Demos how to show help text from an organizational command.
    /// </summary>
    [Command("cmd-defaults", Description = "Showcases default values in help text.")]
    public class DefaultValuesCommand : ICommand
    {
        [CommandOption(nameof(Object))]
        public object? Object { get; set; } = 42;

        [CommandOption(nameof(String))]
        public string? String { get; set; } = "foo";

        [CommandOption(nameof(EmptyString))]
        public string EmptyString { get; set; } = "";

        [CommandOption(nameof(WhiteSpaceString))]
        public string WhiteSpaceString { get; set; } = " ";

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
        public DateTime DateTime { get; set; } = new DateTime(2020, 4, 20);

        [CommandOption(nameof(DateTimeOffset))]
        public DateTimeOffset DateTimeOffset { get; set; } =
            new DateTimeOffset(2008, 5, 1, 0, 0, 0, new TimeSpan(0, 1, 0, 0, 0));

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
        public IReadOnlyList<string>? StringReadOnlyList { get; set; } = new[] { "foo", "bar", "baz" };

        [CommandOption(nameof(StringList))]
        public List<string>? StringList { get; set; } = new List<string>() { "foo", "bar", "baz" };

        public ValueTask ExecuteAsync(IConsole console) =>
            throw new CommandException(null, showHelp: false);
    }
}