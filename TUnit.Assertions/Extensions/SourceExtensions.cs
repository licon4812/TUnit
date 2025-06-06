﻿using System.Runtime.CompilerServices;
using System.Text;
using TUnit.Assertions.AssertConditions;
using TUnit.Assertions.AssertConditions.Interfaces;
using TUnit.Assertions.AssertionBuilders;
using TUnit.Assertions.Assertions.Generics.Conditions;

namespace TUnit.Assertions.Extensions;

public static class SourceExtensions
{
    public static InvokableValueAssertionBuilder<TActual> RegisterAssertion<TActual>(this IValueSource<TActual> source,
        BaseAssertCondition<TActual> assertCondition, string?[] argumentExpressions, [CallerMemberName] string? caller = null)
    {
        if (!string.IsNullOrEmpty(caller))
        {
            source.AppendExpression(BuildExpression(caller, argumentExpressions));
        }
        
        var invokeableAssertionBuilder = source.WithAssertion(assertCondition);

        if (invokeableAssertionBuilder is InvokableValueAssertionBuilder<TActual> invokableValueAssertionBuilder)
        {
            return invokableValueAssertionBuilder;
        }
        
        if(invokeableAssertionBuilder is InvokableAssertionBuilder<TActual> invokableAssertionBuilder)
        {
            return new InvokableValueAssertionBuilder<TActual>(invokableAssertionBuilder);
        }
        
        return new InvokableValueAssertionBuilder<TActual>(new InvokableAssertionBuilder<TActual>(invokeableAssertionBuilder));
    }
    
    public static InvokableValueAssertionBuilder<TToType> RegisterConversionAssertion<TFromType, TToType>(this IValueSource<TFromType> source,
        ConvertToAssertCondition<TFromType, TToType> assertCondition, string?[] argumentExpressions, [CallerMemberName] string? caller = null)
    {
        return new ConvertedValueAssertionBuilder<TFromType, TToType>(source, assertCondition);
    }

    public static InvokableDelegateAssertionBuilder RegisterAssertion<TActual>(this IDelegateSource delegateSource,
        BaseAssertCondition<TActual> assertCondition, string?[] argumentExpressions, [CallerMemberName] string? caller = null)
    {
        if (!string.IsNullOrEmpty(caller))
        {
            delegateSource.AppendExpression(BuildExpression(caller, argumentExpressions));
        }
        
        var source = delegateSource.WithAssertion(assertCondition);

        if (source is InvokableDelegateAssertionBuilder unTypedInvokableDelegateAssertionBuilder)
        {
            return unTypedInvokableDelegateAssertionBuilder;
        }

        if (source is InvokableAssertionBuilder<object?> unTypedInvokableAssertionBuilder)
        {
            return new InvokableDelegateAssertionBuilder(unTypedInvokableAssertionBuilder);
        }

        return new InvokableDelegateAssertionBuilder(new InvokableAssertionBuilder<object?>(source));
    }
    
    public static InvokableValueAssertionBuilder<TToType> RegisterConversionAssertion<TToType>(this IDelegateSource source) where TToType : Exception
    {
        return new ConvertedDelegateAssertionBuilder<TToType>(source);
    }

    private static string BuildExpression(string? caller, string?[] argumentExpressions)
    {
        var assertionBuilder = new StringBuilder();

        argumentExpressions = argumentExpressions.OfType<string>().ToArray();
        
        if(caller is not null)
        {
            assertionBuilder.Append(caller);
        }

        assertionBuilder.Append('(');
        
        for (var index = 0; index < argumentExpressions.Length; index++)
        {
            var argumentExpression = argumentExpressions[index];

            if (string.IsNullOrEmpty(argumentExpression))
            {
                continue;
            }
            
            assertionBuilder.Append(argumentExpression);

            if (index < argumentExpressions.Length - 1)
            {
                assertionBuilder.Append(',');
                assertionBuilder.Append(' ');
            }
        }

        return assertionBuilder.Append(')').ToString();
    }
}