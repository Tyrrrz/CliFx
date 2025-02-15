﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace CliFx.Tests.Utils.Extensions;

internal static class AssertionExtensions
{
    public static void ConsistOfLines(
        this StringAssertions assertions,
        params IEnumerable<string> lines
    ) =>
        assertions
            .Subject.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
            .Should()
            .Equal(lines);

    public static AndConstraint<StringAssertions> ContainAllInOrder(
        this StringAssertions assertions,
        IEnumerable<string> values
    )
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

    public static AndConstraint<StringAssertions> ContainAllInOrder(
        this StringAssertions assertions,
        params string[] values
    ) => assertions.ContainAllInOrder((IEnumerable<string>)values);
}
