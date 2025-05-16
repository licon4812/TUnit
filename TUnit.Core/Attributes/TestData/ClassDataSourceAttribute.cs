using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace TUnit.Core;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
public sealed class ClassDataSourceAttribute<T> : DataSourceGeneratorAttribute<T> where T : new()
{
    public SharedType Shared { get; set; } = SharedType.None;
    public string Key { get; set; } = string.Empty;
    public override IEnumerable<Func<T>> GenerateDataSources(DataGeneratorMetadata dataGeneratorMetadata)
    {
        yield return () =>
        {
            var item = ClassDataSources.Get(dataGeneratorMetadata.TestSessionId)
                .Get<T>(Shared, dataGeneratorMetadata.TestClassType, Key, dataGeneratorMetadata)
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

// Non-generic version for VB.NET and F# compatibility
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
public sealed class ClassDataSourceAttribute(Type dataType) : DataSourceGeneratorAttribute(dataType)
{
    public SharedType Shared { get; set; } = SharedType.None;
    public string Key { get; set; } = string.Empty;

    public override IEnumerable<Func<object>> GenerateDataSources(DataGeneratorMetadata dataGeneratorMetadata)
    {
        var dataType = DataType;
        var shared = Shared;
        var key = Key;

        yield return () =>
        {
            var getInstanceMethod = typeof(ClassDataSources).GetMethod("Get", BindingFlags.Public | BindingFlags.Static, null,
                [typeof(string)], null);
            if (getInstanceMethod == null)
                throw new InvalidOperationException("Could not find ClassDataSources.Get method.");

            var classDataSourcesInstance = getInstanceMethod.Invoke(null, [dataGeneratorMetadata.TestSessionId]);

            var annotatedType = classDataSourcesInstance != null
                ? GetTypeWithAnnotations(classDataSourcesInstance)
                : throw new InvalidOperationException("ClassDataSources instance is null.");

            var getMethod = annotatedType.GetMethod("GetNonGeneric", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (getMethod == null)
                throw new InvalidOperationException("Could not find GetNonGeneric method on ClassDataSources.");

            var item = getMethod.Invoke(classDataSourcesInstance, [
                shared, dataGeneratorMetadata.TestClassType, key, dataGeneratorMetadata, dataType
            ]);

            return item ?? throw new InvalidOperationException("Failed to retrieve a valid data source instance.");
        };
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)]
    private static Type GetTypeWithAnnotations(object instance)
    {
        if (instance is Type annotatedType)
        {
            return annotatedType;
        }

        throw new InvalidOperationException("Instance type does not satisfy required annotations.");
    }
}