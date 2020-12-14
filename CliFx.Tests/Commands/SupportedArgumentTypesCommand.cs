using System;
using System.Collections.Generic;
using System.Globalization;
using CliFx.Attributes;
using Newtonsoft.Json;

namespace CliFx.Tests.Commands
{
    [Command("cmd")]
    public partial class SupportedArgumentTypesCommand : SelfSerializeCommandBase
    {
        [CommandOption("obj")]
        public object? Object { get; set; } = 42;

        [CommandOption("str")]
        public string? String { get; set; } = "foo bar";

        [CommandOption("bool")]
        public bool Bool { get; set; }

        [CommandOption("char")]
        public char Char { get; set; }

        [CommandOption("sbyte")]
        public sbyte Sbyte { get; set; }

        [CommandOption("byte")]
        public byte Byte { get; set; }

        [CommandOption("short")]
        public short Short { get; set; }

        [CommandOption("ushort")]
        public ushort Ushort { get; set; }

        [CommandOption("int")]
        public int Int { get; set; }

        [CommandOption("uint")]
        public uint Uint { get; set; }

        [CommandOption("long")]
        public long Long { get; set; }

        [CommandOption("ulong")]
        public ulong Ulong { get; set; }

        [CommandOption("float")]
        public float Float { get; set; }

        [CommandOption("double")]
        public double Double { get; set; }

        [CommandOption("decimal")]
        public decimal Decimal { get; set; }

        [CommandOption("datetime")]
        public DateTime DateTime { get; set; }

        [CommandOption("datetime-offset")]
        public DateTimeOffset DateTimeOffset { get; set; }

        [CommandOption("timespan")]
        public TimeSpan TimeSpan { get; set; }

        [CommandOption("enum")]
        public CustomEnum Enum { get; set; }

        [CommandOption("int-nullable")]
        public int? IntNullable { get; set; }

        [CommandOption("enum-nullable")]
        public CustomEnum? EnumNullable { get; set; }

        [CommandOption("timespan-nullable")]
        public TimeSpan? TimeSpanNullable { get; set; }

        [CommandOption("str-constructible")]
        public CustomStringConstructible? StringConstructible { get; set; }

        [CommandOption("str-parseable")]
        public CustomStringParseable? StringParseable { get; set; }

        [CommandOption("str-parseable-format")]
        public CustomStringParseableWithFormatProvider? StringParseableWithFormatProvider { get; set; }

        [CommandOption("convertible", Converter = typeof(CustomConvertibleConverter))]
        public CustomConvertible? Convertible { get; set; }

        [CommandOption("obj-array")]
        public object[]? ObjectArray { get; set; }

        [CommandOption("str-array")]
        public string[]? StringArray { get; set; }

        [CommandOption("int-array")]
        public int[]? IntArray { get; set; }

        [CommandOption("enum-array")]
        public CustomEnum[]? EnumArray { get; set; }

        [CommandOption("int-nullable-array")]
        public int?[]? IntNullableArray { get; set; }

        [CommandOption("str-constructible-array")]
        public CustomStringConstructible[]? StringConstructibleArray { get; set; }

        [CommandOption("convertible-array", Converter = typeof(CustomConvertibleConverter))]
        public CustomConvertible[]? ConvertibleArray { get; set; }

        [CommandOption("str-enumerable")]
        public IEnumerable<string>? StringEnumerable { get; set; }

        [CommandOption("str-read-only-list")]
        public IReadOnlyList<string>? StringReadOnlyList { get; set; }

        [CommandOption("str-list")]
        public List<string>? StringList { get; set; }

        [CommandOption("str-set")]
        public HashSet<string>? StringHashSet { get; set; }
    }

    public partial class SupportedArgumentTypesCommand
    {
        public enum CustomEnum
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = 3
        }

        public class CustomStringConstructible
        {
            public string Value { get; }

            public CustomStringConstructible(string value) => Value = value;
        }

        public class CustomStringParseable
        {
            public string Value { get; }

            [JsonConstructor]
            private CustomStringParseable(string value) => Value = value;

            public static CustomStringParseable Parse(string value) => new(value);
        }

        public class CustomStringParseableWithFormatProvider
        {
            public string Value { get; }

            [JsonConstructor]
            private CustomStringParseableWithFormatProvider(string value) => Value = value;

            public static CustomStringParseableWithFormatProvider Parse(string value, IFormatProvider formatProvider) =>
                new(value + " " + formatProvider);
        }

        public class CustomConvertible
        {
            public int Value { get; }

            public CustomConvertible(int value) => Value = value;
        }

        public class CustomConvertibleConverter : ArgumentValueConverter<CustomConvertible>
        {
            public override CustomConvertible ConvertFrom(string value) =>
                new(int.Parse(value, CultureInfo.InvariantCulture));
        }
    }
}