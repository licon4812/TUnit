﻿using TUnit.Assertions.Extensions;
using TUnit.Assertions.Helpers;

namespace TUnit.Assertions.AssertConditions.String;

public class StringNotEqualsExpectedValueAssertCondition(string expected, StringComparison stringComparison)
    : ExpectedValueAssertCondition<string, string>(expected)
{
    internal protected override string GetExpectation()
        => $"to not be equal to {Formatter.Format(ExpectedValue).TruncateWithEllipsis(100)}";

    protected override ValueTask<AssertionResult> GetResult(string? actualValue, string? expectedValue)
    {
        if (actualValue is null)
        {
            return AssertionResult
                .FailIf(expectedValue is null,
                    "it was null");
        }

        return AssertionResult
            .FailIf(string.Equals(actualValue, expectedValue, stringComparison),
                "it was");
    }
}