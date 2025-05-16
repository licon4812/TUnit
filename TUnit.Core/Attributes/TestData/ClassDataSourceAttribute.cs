using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace TUnit.Core;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
public sealed class ClassDataSourceAttribute(Type dataType) : DataSourceGeneratorAttribute(dataType)
{
    public SharedType Shared { get; set; } = SharedType.None;
    public string Key { get; set; } = string.Empty;

    public override IEnumerable<Func<object>> GenerateDataSources(DataGeneratorMetadata dataGeneratorMetadata)
    {
        yield return () =>
        {
            var item = ClassDataSources.Get(dataGeneratorMetadata.TestSessionId)
                .Get<object>(Shared, dataGeneratorMetadata.TestClassType, Key, dataGeneratorMetadata)
                ?? throw new InvalidOperationException("Failed to retrieve a valid data source instance.");

            dataGeneratorMetadata.TestBuilderContext.Current.Events.OnTestRegistered += async (obj, context) =>
            {
                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnTestRegistered(
                    context.TestContext,
                    ClassDataSources.IsStaticProperty(dataGeneratorMetadata),
                    Shared,
                    Key,
                    item);
            };

            dataGeneratorMetadata.TestBuilderContext.Current.Events.OnInitialize += async (obj, context) =>
            {
                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnInitialize(
                    context,
                    ClassDataSources.IsStaticProperty(dataGeneratorMetadata),
                    Shared,
                    Key,
                    item);
            };

            dataGeneratorMetadata.TestBuilderContext.Current.Events.OnTestStart += async (obj, context) =>
            {
                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnTestStart(context, item);
            };

            dataGeneratorMetadata.TestBuilderContext.Current.Events.OnTestEnd += async (obj, context) =>
            {
                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnTestEnd(context, item);
            };

            dataGeneratorMetadata.TestBuilderContext.Current.Events.OnTestSkipped += async (obj, context) =>
            {
                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnDispose(context, Shared, Key, item);
            };

            dataGeneratorMetadata.TestBuilderContext.Current.Events.OnDispose += async (obj, context) =>
            {
                await ClassDataSources.Get(dataGeneratorMetadata.TestSessionId).OnDispose(context, Shared, Key, item);
            };

            return item;
        };
    }
}