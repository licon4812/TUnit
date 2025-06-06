﻿#nullable disable

using System.Collections;
using System.Runtime.CompilerServices;
using TUnit.Assertions.AssertConditions;
using TUnit.Assertions.AssertConditions.Interfaces;
using TUnit.Assertions.AssertionBuilders;

namespace TUnit.Assertions.Extensions;

public static partial class DoesExtensions
{
    public static InvokableValueAssertionBuilder<TDictionary> ContainsKey<TDictionary, TKey>(this IValueSource<TDictionary> valueSource, TKey expected, IEqualityComparer<TKey> equalityComparer = null, [CallerArgumentExpression(nameof(expected))] string doNotPopulateThisValue = null) 
        where TDictionary : IDictionary
    {
        return valueSource.RegisterAssertion(new FuncValueAssertCondition<TDictionary, TKey>(expected,
            (actual, _, _) =>
            {
                Verify.ArgNotNull(actual);
                return actual.Keys.Cast<TKey>().Contains(expected, equalityComparer);
            },
            (_, _, _) => $"The key \"{expected}\" was not found in the dictionary",
            $"contain the key '{expected}'")
            , [doNotPopulateThisValue]);
    }
    
    public static InvokableValueAssertionBuilder<IReadOnlyDictionary<TKey, TValue>> ContainsKey<TKey, TValue>(this IValueSource<IReadOnlyDictionary<TKey, TValue>> valueSource, TKey expected, IEqualityComparer<TKey> equalityComparer = null, [CallerArgumentExpression(nameof(expected))] string doNotPopulateThisValue = null) 
    {
        return valueSource.RegisterAssertion(new FuncValueAssertCondition<IReadOnlyDictionary<TKey, TValue>, TKey>(expected,
                (actual, _, _) =>
                {
                    Verify.ArgNotNull(actual);
                    return actual.Keys.Contains(expected, equalityComparer);
                },
                (_, _, _) => $"The key \"{expected}\" was not found in the dictionary",
                $"contain the key '{expected}'")
            , [doNotPopulateThisValue]);
    }
    
    public static InvokableValueAssertionBuilder<TDictionary> ContainsValue<TDictionary, TValue>(this IValueSource<TDictionary> valueSource, TValue expected, IEqualityComparer<TValue> equalityComparer = null, [CallerArgumentExpression(nameof(expected))] string doNotPopulateThisValue = null) 
        where TDictionary : IDictionary
    {
        return valueSource.RegisterAssertion(new FuncValueAssertCondition<TDictionary, TValue>(expected,
            (actual, _, _) =>
            {
                Verify.ArgNotNull(actual);
                return actual.Values.Cast<TValue>().Contains(expected, equalityComparer);
            },
            (_, _, _) => $"The value \"{expected}\" was not found in the dictionary",
            $"contain the value '{expected}'")
            , [doNotPopulateThisValue]);
    }
}