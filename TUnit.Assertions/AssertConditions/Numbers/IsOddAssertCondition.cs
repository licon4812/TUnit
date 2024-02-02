﻿using System.Numerics;

namespace TUnit.Assertions.AssertConditions.Numbers;

public class IsOddAssertCondition<TActual> : AssertCondition<TActual, TActual>
    where TActual : INumber<TActual>, IModulusOperators<TActual, int, int>
{
    public IsOddAssertCondition(AssertionBuilder<TActual> assertionBuilder) : base(assertionBuilder, default)
    {
    }

    protected override string DefaultMessage => $"{ActualValue} is not odd";

    protected internal override bool Passes(TActual? actualValue, Exception? exception)
    {
        ArgumentNullException.ThrowIfNull(actualValue);
        
        return actualValue % 2 != 0;
    }
}