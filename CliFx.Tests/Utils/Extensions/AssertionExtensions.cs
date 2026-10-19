using System;
using System.Collections.Generic;
using AwesomeAssertions;
using AwesomeAssertions.Primitives;
using AwesomeAssertions.Specialized;

namespace CliFx.Tests.Utils.Extensions;

internal static class AssertionExtensions
{
    // FluentAssertions 8.4 introduced Throw() without a type argument, which AwesomeAssertions doesn't have
    extension<TDelegate, TAssertions>(DelegateAssertions<TDelegate, TAssertions> assertions)
        where TDelegate : Delegate
        where TAssertions : DelegateAssertions<TDelegate, TAssertions>
    {
        public ExceptionAssertions<Exception> Throw(
            string because = "",
            params object[] becauseArgs
        ) => assertions.Throw<Exception>(because, becauseArgs);
    }

    extension(StringAssertions assertions)
    {
        public void ConsistOfLines(params IEnumerable<string> lines) =>
            assertions
                .Subject.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
                .Should()
                .Equal(lines);

        public AndConstraint<StringAssertions> ContainAllInOrder(params IEnumerable<string> values)
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
    }
}
