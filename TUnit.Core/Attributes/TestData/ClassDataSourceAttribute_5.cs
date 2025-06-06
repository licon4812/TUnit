﻿using System.Diagnostics.CodeAnalysis;

namespace TUnit.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class ClassDataSourceAttribute<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] T1,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] T2,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] T3,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] T4,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] T5>
    : DataSourceGeneratorAttribute<T1, T2, T3, T4, T5>, ISharedDataSourceAttribute
    where T1 : new()
    where T2 : new()
    where T3 : new()
    where T4 : new()
    where T5 : new()
{
    public SharedType[] Shared { get; set; } = [SharedType.None, SharedType.None, SharedType.None, SharedType.None, SharedType.None];
    public string[] Keys { get; set; } = [string.Empty, string.Empty, string.Empty, string.Empty, string.Empty];

    public override IEnumerable<Func<(T1, T2, T3, T4, T5)>> GenerateDataSources(DataGeneratorMetadata dataGeneratorMetadata)
    {
        yield return () =>
        {
            (
                (T1 T, SharedType SharedType, string Key),
                (T2 T, SharedType SharedType, string Key),
                (T3 T, SharedType SharedType, string Key),
                (T4 T, SharedType SharedType, string Key),
                (T5 T, SharedType SharedType, string Key)
                ) itemsWithMetadata = (
                    ClassDataSources.Get(dataGeneratorMetadata.TestSessionId)
                        .GetItemForIndex<T1>(0, dataGeneratorMetadata.TestClassType, Shared, Keys, dataGeneratorMetadata),
                    ClassDataSources.Get(dataGeneratorMetadata.TestSessionId)
                        .GetItemForIndex<T2>(1, dataGeneratorMetadata.TestClassType, Shared, Keys, dataGeneratorMetadata),
                    ClassDataSources.Get(dataGeneratorMetadata.TestSessionId)
                        .GetItemForIndex<T3>(2, dataGeneratorMetadata.TestClassType, Shared, Keys, dataGeneratorMetadata),
                    ClassDataSources.Get(dataGeneratorMetadata.TestSessionId)
                        .GetItemForIndex<T4>(3, dataGeneratorMetadata.TestClassType, Shared, Keys, dataGeneratorMetadata),
                    ClassDataSources.Get(dataGeneratorMetadata.TestSessionId)
                        .GetItemForIndex<T5>(4, dataGeneratorMetadata.TestClassType, Shared, Keys, dataGeneratorMetadata)
                );

            dataGeneratorMetadata.TestBuilderContext.Current.Events.OnTestRegistered += async (obj, context) =>
            {
                var testContext = context.TestContext;

                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnTestRegistered(
                    testContext,
                    false,
                    itemsWithMetadata.Item1.SharedType,
                    itemsWithMetadata.Item1.Key,
                    itemsWithMetadata.Item1.T);

                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnTestRegistered(
                    testContext,
                    false,
                    itemsWithMetadata.Item2.SharedType,
                    itemsWithMetadata.Item2.Key,
                    itemsWithMetadata.Item2.T);

                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnTestRegistered(
                    testContext,
                    false,
                    itemsWithMetadata.Item3.SharedType,
                    itemsWithMetadata.Item3.Key,
                    itemsWithMetadata.Item3.T);

                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnTestRegistered(
                    testContext,
                    false,
                    itemsWithMetadata.Item4.SharedType,
                    itemsWithMetadata.Item4.Key,
                    itemsWithMetadata.Item4.T);

                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnTestRegistered(
                    testContext,
                    false,
                    itemsWithMetadata.Item5.SharedType,
                    itemsWithMetadata.Item5.Key,
                    itemsWithMetadata.Item5.T);
            };

            dataGeneratorMetadata.TestBuilderContext.Current.Events.OnInitialize += async (obj, context) =>
            {
                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnInitialize(
                    context,
                    false,
                    itemsWithMetadata.Item1.SharedType,
                    itemsWithMetadata.Item1.Key,
                    itemsWithMetadata.Item1.T);

                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnInitialize(
                    context,
                    false,
                    itemsWithMetadata.Item2.SharedType,
                    itemsWithMetadata.Item2.Key,
                    itemsWithMetadata.Item2.T);

                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnInitialize(
                    context,
                    false,
                    itemsWithMetadata.Item3.SharedType,
                    itemsWithMetadata.Item3.Key,
                    itemsWithMetadata.Item3.T);

                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnInitialize(
                    context,
                    false,
                    itemsWithMetadata.Item4.SharedType,
                    itemsWithMetadata.Item4.Key,
                    itemsWithMetadata.Item4.T);

                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnInitialize(
                    context,
                    false,
                    itemsWithMetadata.Item5.SharedType,
                    itemsWithMetadata.Item5.Key,
                    itemsWithMetadata.Item5.T);
            };

            dataGeneratorMetadata.TestBuilderContext.Current.Events.OnTestStart += async (obj, context) =>
            {
                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnTestStart(context, itemsWithMetadata.Item1.T);
                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnTestStart(context, itemsWithMetadata.Item2.T);
                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnTestStart(context, itemsWithMetadata.Item2.T);
                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnTestStart(context, itemsWithMetadata.Item4.T);
                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnTestStart(context, itemsWithMetadata.Item5.T);
            };

            dataGeneratorMetadata.TestBuilderContext.Current.Events.OnTestEnd += async (obj, context) =>
            {
                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnTestEnd(context, itemsWithMetadata.Item1.T);
                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnTestEnd(context, itemsWithMetadata.Item2.T);
                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnTestEnd(context, itemsWithMetadata.Item3.T);
                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnTestEnd(context, itemsWithMetadata.Item4.T);
                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnTestEnd(context, itemsWithMetadata.Item5.T);
            };

            dataGeneratorMetadata.TestBuilderContext.Current.Events.OnTestSkipped += async (obj, context) =>
            {
                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnDispose(
                    context,
                    itemsWithMetadata.Item1.SharedType,
                    itemsWithMetadata.Item1.Key,
                    itemsWithMetadata.Item1.T);

                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnDispose(
                    context,
                    itemsWithMetadata.Item2.SharedType,
                    itemsWithMetadata.Item2.Key,
                    itemsWithMetadata.Item2.T);

                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnDispose(
                    context,
                    itemsWithMetadata.Item3.SharedType,
                    itemsWithMetadata.Item3.Key,
                    itemsWithMetadata.Item3.T);

                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnDispose(
                    context,
                    itemsWithMetadata.Item4.SharedType,
                    itemsWithMetadata.Item4.Key,
                    itemsWithMetadata.Item4.T);

                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnDispose(
                    context,
                    itemsWithMetadata.Item5.SharedType,
                    itemsWithMetadata.Item5.Key,
                    itemsWithMetadata.Item5.T);
            };

            dataGeneratorMetadata.TestBuilderContext.Current.Events.OnDispose += async (obj, context) =>
            {
                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnDispose(
                    context,
                    itemsWithMetadata.Item1.SharedType,
                    itemsWithMetadata.Item1.Key,
                    itemsWithMetadata.Item1.T);

                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnDispose(
                    context,
                    itemsWithMetadata.Item2.SharedType,
                    itemsWithMetadata.Item2.Key,
                    itemsWithMetadata.Item2.T);

                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnDispose(
                    context,
                    itemsWithMetadata.Item3.SharedType,
                    itemsWithMetadata.Item3.Key,
                    itemsWithMetadata.Item3.T);

                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnDispose(
                    context,
                    itemsWithMetadata.Item4.SharedType,
                    itemsWithMetadata.Item4.Key,
                    itemsWithMetadata.Item4.T);

                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnDispose(
                    context,
                    itemsWithMetadata.Item5.SharedType,
                    itemsWithMetadata.Item5.Key,
                    itemsWithMetadata.Item5.T);
            };

            return (
                itemsWithMetadata.Item1.T,
                itemsWithMetadata.Item2.T,
                itemsWithMetadata.Item3.T,
                itemsWithMetadata.Item4.T,
                itemsWithMetadata.Item5.T
            );
        };
    }

    public IEnumerable<SharedType> GetSharedTypes() => Shared;

    public IEnumerable<string> GetKeys() => Keys;
}
