﻿namespace TUnit.Assertions.AssertConditions.Comparable;

public class BetweenAssertCondition<TActual>(TActual minimum, TActual maximum) : BaseAssertCondition<TActual> 
    where TActual : IComparable<TActual>
{
    private bool _inclusiveBounds;

    internal protected override string GetExpectation() => $"to be between {minimum} & {maximum} ({GetRange()} Range)";

    protected override ValueTask<AssertionResult> GetResult(
        TActual? actualValue, Exception? exception,
        AssertionMetadata assertionMetadata
    )
    {
        bool isInRange;

        if (_inclusiveBounds)
        {
            isInRange = actualValue!.CompareTo(minimum) >= 0 && actualValue.CompareTo(maximum) <= 0;
        }
        else
        {
            isInRange = actualValue!.CompareTo(minimum) > 0 && actualValue.CompareTo(maximum) < 0;
        }

        return AssertionResult
            .FailIf(!isInRange,
                $"received {actualValue}");

    }

    public void Inclusive()
    {
        _inclusiveBounds = true;
    }

    public void Exclusive()
    {
        _inclusiveBounds = false;
    }

    private string GetRange()
    {
        return _inclusiveBounds ? "Inclusive" : "Exclusive";
    }
}