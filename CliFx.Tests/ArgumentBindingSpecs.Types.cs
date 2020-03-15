﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace CliFx.Tests
{
    public partial class ArgumentBindingSpecs
    {
        private enum CustomEnum
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = 3
        }

        private class StringConstructable
        {
            public string Value { get; }

            public StringConstructable(string value)
            {
                Value = value;
            }
        }

        private class StringParseable
        {
            public string Value { get; }

            private StringParseable(string value)
            {
                Value = value;
            }

            public static StringParseable Parse(string value) => new StringParseable(value);
        }

        private class StringParseableWithFormatProvider
        {
            public string Value { get; }

            private StringParseableWithFormatProvider(string value)
            {
                Value = value;
            }

            public static StringParseableWithFormatProvider Parse(string value, IFormatProvider formatProvider) =>
                new StringParseableWithFormatProvider(value + " " + formatProvider);
        }

        private class NonStringParseable
        {
            public int Value { get; }

            public NonStringParseable(int value)
            {
                Value = value;
            }
        }

        public class CustomEnumerable<T> : IEnumerable<T>
        {
            private readonly T[] _arr = new T[0];

            public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>) _arr).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}