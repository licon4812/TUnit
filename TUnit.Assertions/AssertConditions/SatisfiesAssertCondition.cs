﻿using TUnit.Assertions.AssertConditions.Interfaces;
using TUnit.Assertions.AssertionBuilders;

namespace TUnit.Assertions.AssertConditions;

public class SatisfiesAssertCondition<TActual, TExpected> : BaseAssertCondition<TActual>
{
    private readonly Func<TActual, Task<TExpected>?> _mapper;
    private readonly Func<IValueSource<TExpected?>, InvokableAssertionBuilder<TExpected?>> _assertionBuilder;
    private readonly string _assertionBuilderExpression;

    public SatisfiesAssertCondition(Func<TActual, Task<TExpected>?> mapper,
        Func<IValueSource<TExpected?>, InvokableAssertionBuilder<TExpected?>> assertionBuilder, string mapperExpression, string assertionBuilderExpression)
    {
        _mapper = mapper;
        _assertionBuilder = assertionBuilder;
        _assertionBuilderExpression = assertionBuilderExpression;
        
        SetSubject(mapperExpression);
    }

    internal protected override string GetExpectation()
        => $"to satisfy {_assertionBuilderExpression}";

    protected override async ValueTask<AssertionResult> GetResult(
        TActual? actualValue, Exception? exception,
        AssertionMetadata assertionMetadata
    )
    {
        if (actualValue is null)
        {
            return AssertionResult.Fail("is null");
        }
        
        var innerItemTask = _mapper(actualValue);

        var innerItem = innerItemTask == null ? default : await innerItemTask;
        
        var innerAssertionBuilder = new ValueAssertionBuilder<TExpected?>(innerItem, "");

        var assertion = _assertionBuilder(innerAssertionBuilder);
        
        foreach (var baseAssertCondition in ((ISource)assertion).Assertions)
        {
            var result = await baseAssertCondition.GetAssertionResult(innerItem, exception, assertionMetadata, "");

            if (!result.IsPassed)
            {
                return result;
            }
        }
        
        return AssertionResult.Passed;
    }
}