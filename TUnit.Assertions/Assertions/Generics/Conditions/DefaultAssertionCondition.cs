﻿using TUnit.Assertions.AssertConditions;

namespace TUnit.Assertions.Assertions.Generics.Conditions;

public class DefaultExpectedValueAssertCondition<TActual> : BaseAssertCondition<TActual>
{
    private readonly TActual? _defaultValue = default;

    internal protected override string GetExpectation()
        => $"to be {(_defaultValue is null ? "null" : _defaultValue)}";

    protected override ValueTask<AssertionResult> GetResult(
        TActual? actualValue, Exception? exception,
        AssertionMetadata assertionMetadata
    )
        => AssertionResult
            .FailIf(actualValue is not null && !actualValue.Equals(_defaultValue),
                $"found {actualValue}");
}