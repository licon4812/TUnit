﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TUnit.Core.SourceGenerator.CodeGenerators.Helpers;
using TUnit.Core.SourceGenerator.Extensions;

namespace TUnit.Core.SourceGenerator.CodeGenerators.Writers;

public class AttributeWriter
{
    public static void WriteAttributes(SourceCodeWriter sourceCodeWriter, GeneratorAttributeSyntaxContext context,
        ImmutableArray<AttributeData> attributeDatas)
    {
        var dataAttributeInterface =
            context.SemanticModel.Compilation.GetTypeByMetadataName(WellKnownFullyQualifiedClassNames.IDataSourceGeneratorAttribute
                .WithoutGlobalPrefix);
        
        attributeDatas = attributeDatas.RemoveAll(x => x.AttributeClass?.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, dataAttributeInterface)) == true);
        
        if (attributeDatas.Length == 0)
        {
            sourceCodeWriter.Write("[],");
            sourceCodeWriter.WriteLine();
            return;
        }

        sourceCodeWriter.WriteLine();
        sourceCodeWriter.WriteLine("[");
        for (var index = 0; index < attributeDatas.Length; index++)
        {
            sourceCodeWriter.WriteTabs();
            var attributeData = attributeDatas[index];

            if (attributeData.ApplicationSyntaxReference is null)
            {
                continue;
            }

            WriteAttribute(sourceCodeWriter, context, attributeData);

            if (index != attributeDatas.Length - 1)
            {
                sourceCodeWriter.Write(",");
            }

            sourceCodeWriter.WriteLine();
        }
        sourceCodeWriter.WriteLine("],");
    }

    public static void WriteAttribute(SourceCodeWriter sourceCodeWriter, GeneratorAttributeSyntaxContext context,
        AttributeData attributeData)
    {
        var syntax = attributeData.ApplicationSyntaxReference?.GetSyntax();

        if (syntax is null)
        {
            WriteAttributeWithoutSyntax(sourceCodeWriter, context, attributeData);
            return;
        }

        var arguments = syntax.ChildNodes()
            .OfType<AttributeArgumentListSyntax>()
            .FirstOrDefault()
            ?.Arguments ?? [];

        var properties = arguments.Where(x => x.NameEquals != null);

        var constructorArgs = arguments.Where(x => x.NameEquals == null);

        var attributeName = attributeData.AttributeClass!.GloballyQualified();

        var formattedConstructorArgs = string.Join(", ", constructorArgs.Select(x => FormatConstructorArgument(context, x)));

        var formattedProperties = properties.Select(x => FormatProperty(context, x)).ToArray();

        sourceCodeWriter.Write($"new {attributeName}({formattedConstructorArgs})");

        if (formattedProperties.Length == 0)
        {
            return;
        }

        sourceCodeWriter.WriteLine();
        sourceCodeWriter.WriteLine("{");
        foreach (var property in formattedProperties)
        {
            sourceCodeWriter.WriteLine($"{property},");
        }

        sourceCodeWriter.Write("}");
    }

    private static string FormatConstructorArgument(GeneratorAttributeSyntaxContext context, AttributeArgumentSyntax attributeArgumentSyntax)
    {
        if (attributeArgumentSyntax.NameColon is not null)
        {
            return $"{attributeArgumentSyntax.NameColon!.Name}: {attributeArgumentSyntax.Expression.Accept(new FullyQualifiedWithGlobalPrefixRewriter(context.SemanticModel))!.ToFullString()}";
        }

        return attributeArgumentSyntax.Accept(new FullyQualifiedWithGlobalPrefixRewriter(context.SemanticModel))!.ToFullString();
    }

    private static string FormatProperty(GeneratorAttributeSyntaxContext context, AttributeArgumentSyntax attributeArgumentSyntax)
    {
        return $"{attributeArgumentSyntax.NameEquals!.Name} = {attributeArgumentSyntax.Expression.Accept(new FullyQualifiedWithGlobalPrefixRewriter(context.SemanticModel))!.ToFullString()}";
    }

    public static void WriteAttributeWithoutSyntax(SourceCodeWriter sourceCodeWriter, GeneratorAttributeSyntaxContext context,
    AttributeData attributeData)
    {
        var attributeName = attributeData.AttributeClass!.GloballyQualified();

        var constructorArgs = attributeData.ConstructorArguments.Select(TypedConstantParser.GetRawTypedConstantValue);
        var formattedConstructorArgs = string.Join(", ", constructorArgs);

        var namedArgs = attributeData.NamedArguments.Select(arg => $"{arg.Key} = {TypedConstantParser.GetRawTypedConstantValue(arg.Value)}");
        var formattedNamedArgs = string.Join(", ", namedArgs);

        sourceCodeWriter.Write($"new {attributeName}({formattedConstructorArgs})");

        if (string.IsNullOrEmpty(formattedNamedArgs))
        {
            return;
        }

        sourceCodeWriter.WriteLine();
        sourceCodeWriter.WriteLine("{");
        sourceCodeWriter.WriteLine($"{formattedNamedArgs}");
        sourceCodeWriter.Write("}");
    }
}