﻿using Microsoft.CodeAnalysis;
using TUnit.Core.SourceGenerator.CodeGenerators.Helpers;
using TUnit.Core.SourceGenerator.CodeGenerators.Writers;
using TUnit.Core.SourceGenerator.Extensions;
using TUnit.Core.SourceGenerator.Models;

namespace TUnit.Core.SourceGenerator.CodeGenerators;

[Generator]
public class TestsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var standardTests = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "TUnit.Core.TestAttribute",
                predicate: static (_, _) => true,
                transform: static (ctx, _) => GetSemanticTargetForTestMethodGeneration(ctx))
            .Where(static m => m is not null);

        var inheritedTests = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "TUnit.Core.InheritsTestsAttribute",
                predicate: static (_, _) => true,
                transform: static (ctx, _) => GetSemanticTargetForInheritedTestsGeneration(ctx))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(standardTests, (sourceContext, data) => GenerateTests(sourceContext, data!));
        context.RegisterSourceOutput(inheritedTests, (sourceContext, data) => GenerateTests(sourceContext, data!, "Inherited_"));
    }

    static TestCollectionDataModel? GetSemanticTargetForTestMethodGeneration(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        if (methodSymbol.ContainingType.IsAbstract)
        {
            return null;
        }

        if (methodSymbol.IsStatic)
        {
            return null;
        }

        if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
        {
            return null;
        }

        if (methodSymbol.ContainingType.IsGenericDefinition())
        {
            return null;
        }

        return new TestCollectionDataModel(methodSymbol.ParseTestDatas(context, methodSymbol.ContainingType));
    }

    static TestCollectionDataModel? GetSemanticTargetForInheritedTestsGeneration(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return null;
        }

        if (namedTypeSymbol.IsAbstract)
        {
            return null;
        }

        if (namedTypeSymbol.IsStatic)
        {
            return null;
        }

        if (namedTypeSymbol.DeclaredAccessibility != Accessibility.Public)
        {
            return null;
        }

        if (namedTypeSymbol.IsGenericDefinition())
        {
            return null;
        }

        return new TestCollectionDataModel(
            namedTypeSymbol.GetBaseTypes()
                .SelectMany(x => x.GetMembers())
                .OfType<IMethodSymbol>()
                .Where(x => !x.IsAbstract)
                .Where(x => x.MethodKind != MethodKind.Constructor)
                .Where(x => x.IsTest())
                .SelectMany(x => x.ParseTestDatas(context, namedTypeSymbol))
        );
    }

    private void GenerateTests(SourceProductionContext context, TestCollectionDataModel testCollection, string? prefix = null)
    {
        try
        {
            foreach (var classGrouping in testCollection
                         .TestSourceDataModels
                         .GroupBy(x => $"{prefix}{x.ClassNameToGenerate}"))
            {
                var className = classGrouping.Key;
                var count = classGrouping.Count();

                using var sourceBuilder = new SourceCodeWriter();

                sourceBuilder.WriteLine("// <auto-generated/>");
                sourceBuilder.WriteLine("#pragma warning disable");
                sourceBuilder.WriteLine("using global::System.Linq;");
                sourceBuilder.WriteLine("using global::System.Reflection;");
                sourceBuilder.WriteLine("using global::TUnit.Core;");
                sourceBuilder.WriteLine("using global::TUnit.Core.Extensions;");
                sourceBuilder.WriteLine();
                sourceBuilder.WriteLine("namespace TUnit.SourceGenerated;");
                sourceBuilder.WriteLine();
                sourceBuilder.WriteLine("[global::System.Diagnostics.StackTraceHidden]");
                sourceBuilder.WriteLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
                sourceBuilder.WriteLine(
                    $"file partial class {className} : global::TUnit.Core.Interfaces.SourceGenerator.ITestSource");
                sourceBuilder.WriteLine("{");

                sourceBuilder.WriteLine("[global::System.Runtime.CompilerServices.ModuleInitializer]");
                sourceBuilder.WriteLine("public static void Initialise()");
                sourceBuilder.WriteLine("{");
                sourceBuilder.WriteLine($"global::TUnit.Core.SourceRegistrar.Register(new {className}());");
                sourceBuilder.WriteLine("}");

                sourceBuilder.WriteLine(
                    "public global::System.Collections.Generic.IReadOnlyList<TestMetadata> CollectTests(string sessionId)");
                sourceBuilder.WriteLine("{");
                if (count == 1)
                {
                    sourceBuilder.WriteLine("return Tests0(sessionId);");
                }
                else
                {
                    sourceBuilder.WriteLine("return");
                    sourceBuilder.WriteLine("[");
                    for (var i = 0; i < count; i++)
                    {
                        sourceBuilder.WriteLine($"..Tests{i}(sessionId),");
                    }

                    sourceBuilder.WriteLine("];");
                }

                sourceBuilder.WriteLine("}");

                var index = 0;
                foreach (var model in classGrouping)
                {
                    sourceBuilder.WriteLine(
                        $"private global::System.Collections.Generic.List<TestMetadata> Tests{index++}(string sessionId)");
                    sourceBuilder.WriteLine("{");
                    sourceBuilder.WriteLine(
                        "global::System.Collections.Generic.List<TestMetadata> nodes = [];");
                    sourceBuilder.WriteLine($"var {VariableNames.ClassDataIndex} = 0;");
                    sourceBuilder.WriteLine($"var {VariableNames.TestMethodDataIndex} = 0;");

                    sourceBuilder.WriteLine("try");
                    sourceBuilder.WriteLine("{");
                    GenericTestInvocationWriter.GenerateTestInvocationCode(context, sourceBuilder, model);
                    sourceBuilder.WriteLine("}");
                    sourceBuilder.WriteLine("catch (global::System.Exception exception)");
                    sourceBuilder.WriteLine("{");
                    FailedTestInitializationWriter.GenerateFailedTestCode(sourceBuilder, model);
                    sourceBuilder.WriteLine("}");

                    sourceBuilder.WriteLine("return nodes;");
                    sourceBuilder.WriteLine("}");
                }

                sourceBuilder.WriteLine("}");

                context.AddSource($"{className}-{Guid.NewGuid():N}.Generated.cs", sourceBuilder.ToString());
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

            context.ReportDiagnostic(Diagnostic.Create(descriptor, null, ex.ToString()));
        }
    }
}
