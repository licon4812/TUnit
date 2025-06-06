﻿using Inject.NET.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TUnit.Core.SourceGenerator.CodeGenerators.Writers.Hooks;
using TUnit.Core.SourceGenerator.Enums;
using TUnit.Core.SourceGenerator.Extensions;
using TUnit.Core.SourceGenerator.Models;

namespace TUnit.Core.SourceGenerator.CodeGenerators;

[Generator]
public class TestHooksGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var setUpMethods = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "TUnit.Core.BeforeAttribute",
                predicate: static (_, _) => true,
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx, HookLocationType.Before, false))
            .Where(static m => m is not null);

        var cleanUpMethods = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "TUnit.Core.AfterAttribute",
                predicate: static (_, _) => true,
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx, HookLocationType.After, false))
            .Where(static m => m is not null);

        var beforeEveryMethods = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "TUnit.Core.BeforeEveryAttribute",
                predicate: static (s, _) => true,
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx, HookLocationType.Before, true))
            .Where(static m => m is not null);

        var afterEveryMethods = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "TUnit.Core.AfterEveryAttribute",
                predicate: static (s, _) => true,
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx, HookLocationType.After, true))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(setUpMethods, Generate);
        context.RegisterSourceOutput(cleanUpMethods, Generate);
        context.RegisterSourceOutput(beforeEveryMethods, Generate);
        context.RegisterSourceOutput(afterEveryMethods, Generate);
    }

    static IEnumerable<HooksDataModel> GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context,
        HookLocationType hookLocationType, bool isEveryHook)
    {
        if (context.TargetSymbol is not IMethodSymbol methodSymbol)
        {
            yield break;
        }

        var containingType = methodSymbol.ContainingType;
        IEnumerable<INamedTypeSymbol> classTypes;

        if (containingType.IsGenericDefinition())
        {
            classTypes = [containingType.ConstructUnboundGenericType(), ..GenericTypeHelper.GetConstructedTypes(context.SemanticModel.Compilation, containingType)];
        }
        else
        {
            classTypes = [containingType];
        }

        foreach (var classType in classTypes)
        {
            foreach (var contextAttribute in context.Attributes)
            {
                var hookLevel = contextAttribute.ConstructorArguments[0].ToCSharpString();

                yield return new HooksDataModel
                {
                    Context = context,
                    Method = methodSymbol,
                    MethodName = methodSymbol.Name,
                    HookLocationType = hookLocationType,
                    IsEveryHook = isEveryHook && hookLevel is not "TUnit.Core.HookType.TestDiscovery" and not "TUnit.Core.HookType.TestSession",
                    HookLevel = hookLevel,
                    ClassType = classType,
                    FullyQualifiedTypeName =
                        classType.GloballyQualified(),
                    MinimalTypeName = classType.Name,
                    ParameterTypes = methodSymbol.Parameters
                        .Select(x => x.Type.GloballyQualified())
                        .ToArray(),
                    HookExecutor = methodSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.IsOrInherits("global::TUnit.Core.Executors.HookExecutorAttribute") == true)?.AttributeClass?.TypeArguments.FirstOrDefault()?.GloballyQualified(),
                    Order = contextAttribute.NamedArguments.FirstOrDefault(x => x.Key == "Order").Value.Value as int? ?? 0,
                    FilePath = contextAttribute.ConstructorArguments[1].Value?.ToString() ?? string.Empty,
                    LineNumber = contextAttribute.ConstructorArguments[2].Value as int? ?? 0,
                    IsVoid = methodSymbol.ReturnsVoid,
                };
            }
        }
    }

    private void Generate(SourceProductionContext productionContext, IEnumerable<HooksDataModel> hooks)
    {
        try
        {
            foreach (var groupedByTypeName in hooks.GroupBy(x => x.MinimalTypeName))
            {
                var className = $"Hooks_{groupedByTypeName.Key}";

                using var sourceBuilder = new SourceCodeWriter();

                sourceBuilder.WriteLine("// <auto-generated/>");
                sourceBuilder.WriteLine("#pragma warning disable");
                sourceBuilder.WriteLine("using global::System.Linq;");
                sourceBuilder.WriteLine("using global::System.Reflection;");
                sourceBuilder.WriteLine("using global::System.Runtime.CompilerServices;");
                sourceBuilder.WriteLine("using global::TUnit.Core;");
                sourceBuilder.WriteLine("using global::TUnit.Core.Hooks;");
                sourceBuilder.WriteLine("using global::TUnit.Core.Interfaces;");
                sourceBuilder.WriteLine();
                sourceBuilder.WriteLine("namespace TUnit.SourceGenerated;");
                sourceBuilder.WriteLine();
                sourceBuilder.WriteLine("[global::System.Diagnostics.StackTraceHidden]");
                sourceBuilder.WriteLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");

                var distinctHookLevelsForClass = groupedByTypeName.Select(x => x.HookLevel).Distinct().ToList();

                sourceBuilder.WriteLine(
                    $"file partial class {className} : {string.Join(", ", distinctHookLevelsForClass.Select(GetInterfaceType))}");
                sourceBuilder.WriteLine("{");
                sourceBuilder.WriteLine("[global::System.Runtime.CompilerServices.ModuleInitializer]");
                sourceBuilder.WriteLine("public static void Initialise()");
                sourceBuilder.WriteLine("{");

                sourceBuilder.WriteLine($"var instance = new {className}();");
                foreach (var hookLevel in distinctHookLevelsForClass)
                {
                    sourceBuilder.WriteLine($"SourceRegistrar.{GetSourceRegisterMethodName(hookLevel)}(instance);");
                }

                sourceBuilder.WriteLine("}");

                foreach (var hooksGroupedByLevel in groupedByTypeName.GroupBy(x => x.HookLevel))
                {
                    foreach (var isEvery in new[] { true, false })
                    {
                        if (isEvery && hooksGroupedByLevel.Key
                                is "TUnit.Core.HookType.TestDiscovery"
                                or "TUnit.Core.HookType.TestSession")
                        {
                            // These don't have an 'isEvery' option
                            continue;
                        }

                        foreach (var hookLocationType in new[] { HookLocationType.Before, HookLocationType.After })
                        {
                            sourceBuilder.WriteLine(
                                $"public global::System.Collections.Generic.IReadOnlyList<{GetReturnType(hooksGroupedByLevel.Key, hookLocationType, isEvery)}> {GetMethodName(hooksGroupedByLevel.Key, hookLocationType, isEvery)}(string sessionId)");

                            sourceBuilder.WriteLine("{");
                            sourceBuilder.WriteLine("return");
                            sourceBuilder.WriteLine("[");

                            foreach (var model in hooksGroupedByLevel.Where(x =>
                                         x.HookLocationType == hookLocationType && x.IsEveryHook == isEvery))
                            {
                                if (hooksGroupedByLevel.Key == "TUnit.Core.HookType.Test")
                                {
                                    TestHooksWriter.Execute(sourceBuilder, model);
                                }
                                else if (hooksGroupedByLevel.Key == "TUnit.Core.HookType.Class")
                                {
                                    ClassHooksWriter.Execute(sourceBuilder, model);
                                }
                                else if (hooksGroupedByLevel.Key == "TUnit.Core.HookType.Assembly")
                                {
                                    AssemblyHooksWriter.Execute(sourceBuilder, model);
                                }
                                else if (hooksGroupedByLevel.Key is "TUnit.Core.HookType.TestDiscovery"
                                         or "TUnit.Core.HookType.TestSession")
                                {
                                    GlobalTestHooksWriter.Execute(sourceBuilder, model);
                                }
                            }

                            sourceBuilder.WriteLine("];");
                            sourceBuilder.WriteLine("}");
                        }
                    }
                }

                sourceBuilder.WriteLine("}");

                productionContext.AddSource($"{className}-{Guid.NewGuid():N}.Generated.cs", sourceBuilder.ToString());
            }
        }
        catch (Exception ex)
        {
            var descriptor = new DiagnosticDescriptor(id: "TUnit0000",
                title: "Error Generating Source",
                messageFormat: "{0}",
                category: "SourceGenerator",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

            productionContext.ReportDiagnostic(Diagnostic.Create(descriptor, null, ex.ToString()));
        }
    }

    private string GetSourceRegisterMethodName(string hookLevel)
    {
        return hookLevel switch
        {
            "TUnit.Core.HookType.TestDiscovery" => "RegisterTestDiscoveryHookSource",
            "TUnit.Core.HookType.TestSession" => "RegisterTestSessionHookSource",
            "TUnit.Core.HookType.Assembly" => "RegisterAssemblyHookSource",
            "TUnit.Core.HookType.Class" => "RegisterClassHookSource",
            "TUnit.Core.HookType.Test" => "RegisterTestHookSource",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static string GetInterfaceType(string hookLevel)
    {
        return hookLevel switch
        {
            "TUnit.Core.HookType.TestDiscovery" => "global::TUnit.Core.Interfaces.SourceGenerator.ITestDiscoveryHookSource",
            "TUnit.Core.HookType.TestSession" => "global::TUnit.Core.Interfaces.SourceGenerator.ITestSessionHookSource",
            "TUnit.Core.HookType.Assembly" => "global::TUnit.Core.Interfaces.SourceGenerator.IAssemblyHookSource",
            "TUnit.Core.HookType.Class" => "global::TUnit.Core.Interfaces.SourceGenerator.IClassHookSource",
            "TUnit.Core.HookType.Test" => "global::TUnit.Core.Interfaces.SourceGenerator.ITestHookSource",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static string GetReturnType(string hookLevel, HookLocationType hookLocationType, bool isEvery)
    {
        return hookLevel switch
        {
            "TUnit.Core.HookType.TestDiscovery"
                when hookLocationType == HookLocationType.Before => "global::TUnit.Core.Hooks.StaticHookMethod<global::TUnit.Core.BeforeTestDiscoveryContext>",
            "TUnit.Core.HookType.TestDiscovery" => "global::TUnit.Core.Hooks.StaticHookMethod<global::TUnit.Core.TestDiscoveryContext>",
            "TUnit.Core.HookType.TestSession" => "global::TUnit.Core.Hooks.StaticHookMethod<global::TUnit.Core.TestSessionContext>",
            "TUnit.Core.HookType.Assembly" => "global::TUnit.Core.Hooks.StaticHookMethod<global::TUnit.Core.AssemblyHookContext>",
            "TUnit.Core.HookType.Class" => "global::TUnit.Core.Hooks.StaticHookMethod<global::TUnit.Core.ClassHookContext>",
            "TUnit.Core.HookType.Test" when isEvery => "global::TUnit.Core.Hooks.StaticHookMethod<global::TUnit.Core.TestContext>",
            "TUnit.Core.HookType.Test" => "global::TUnit.Core.Hooks.InstanceHookMethod",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static string GetMethodName(string hookLevel, HookLocationType hookLocationType, bool isEvery)
    {
        return hookLevel switch
        {
            "TUnit.Core.HookType.TestDiscovery"
                when hookLocationType == HookLocationType.Before => "CollectBeforeTestDiscoveryHooks",
            "TUnit.Core.HookType.TestDiscovery"
                when hookLocationType == HookLocationType.After => "CollectAfterTestDiscoveryHooks",

            "TUnit.Core.HookType.TestSession"
                when hookLocationType == HookLocationType.Before => "CollectBeforeTestSessionHooks",
            "TUnit.Core.HookType.TestSession"
                when hookLocationType == HookLocationType.After => "CollectAfterTestSessionHooks",

            "TUnit.Core.HookType.Assembly"
                when hookLocationType == HookLocationType.Before && isEvery => "CollectBeforeEveryAssemblyHooks",
            "TUnit.Core.HookType.Assembly"
                when hookLocationType == HookLocationType.Before => "CollectBeforeAssemblyHooks",
            "TUnit.Core.HookType.Assembly"
                when hookLocationType == HookLocationType.After && isEvery => "CollectAfterEveryAssemblyHooks",
            "TUnit.Core.HookType.Assembly"
                when hookLocationType == HookLocationType.After => "CollectAfterAssemblyHooks",

            "TUnit.Core.HookType.Class"
                when hookLocationType == HookLocationType.Before && isEvery => "CollectBeforeEveryClassHooks",
            "TUnit.Core.HookType.Class"
                when hookLocationType == HookLocationType.Before => "CollectBeforeClassHooks",
            "TUnit.Core.HookType.Class"
                when hookLocationType == HookLocationType.After && isEvery => "CollectAfterEveryClassHooks",
            "TUnit.Core.HookType.Class"
                when hookLocationType == HookLocationType.After => "CollectAfterClassHooks",

            "TUnit.Core.HookType.Test"
                when hookLocationType == HookLocationType.Before && isEvery => "CollectBeforeEveryTestHooks",
            "TUnit.Core.HookType.Test"
                when hookLocationType == HookLocationType.Before => "CollectBeforeTestHooks",
            "TUnit.Core.HookType.Test"
                when hookLocationType == HookLocationType.After && isEvery => "CollectAfterEveryTestHooks",
            "TUnit.Core.HookType.Test"
                when hookLocationType == HookLocationType.After => "CollectAfterTestHooks",

            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
