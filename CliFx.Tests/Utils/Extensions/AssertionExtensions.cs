using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace CliFx.Tests.Utils.Extensions;

internal static class AssertionExtensions
{
    extension(StringAssertions assertions)
    {
        public void ConsistOfLines(params IEnumerable<string> lines) =>
            assertions
                .Subject.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
                .Should()
                .Equal(lines);

        public AndConstraint<StringAssertions> ContainAllInOrder(IEnumerable<string> values)
        {
            var lastIndex = 0;

            foreach (var value in values)
            {
                var index = assertions.Subject.IndexOf(value, lastIndex, StringComparison.Ordinal);

                if (index < 0)
                {
                    assertions.CurrentAssertionChain.FailWith(
                        $"Expected string '{assertions.Subject}' to contain '{value}' after position {lastIndex}."
                    );
                }

                lastIndex = index;
            }

            return new AndConstraint<StringAssertions>(assertions);
        }

        public AndConstraint<StringAssertions> ContainAllInOrder(params string[] values) =>
            assertions.ContainAllInOrder((IEnumerable<string>)values);
    }
}
