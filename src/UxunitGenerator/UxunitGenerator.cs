// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.Diagnostics;

namespace XUnitWrapperGenerator;

[Generator]
public sealed class XUnitWrapperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var testMethodsProvider = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => IsTestMethodFast(node),
                static (context, _) => TryGetTestMethod(context))
            .Where(mOpt => mOpt is not null)
            .Select<TestMethod?, TestMethod>((mOpt, _) => mOpt.GetValueOrDefault())
            .Collect()
            .Select(static (methods, _) =>
            {
                var methodsByType = new Dictionary<string, List<string>>();
                foreach (var m in methods)
                {
                    if (methodsByType.TryGetValue(m.ContainingTypeName, out var methodNames))
                    {
                        methodNames.Add(m.MethodName);
                    }
                    else
                    {
                        methodsByType.Add(m.ContainingTypeName, new List<string>() { m.MethodName });
                    }
                }
                return new TestCollection(methodsByType);
            });

        context.RegisterSourceOutput(testMethodsProvider, static (prodCtx, testCollection) =>
        {
            foreach (var (typeName, methodNames) in testCollection.MethodsByType)
            {
                var uxunitTestStruct = $$"""
namespace Uxunit;
internal static class {{typeName}}UxunitTests
{
    public static readonly (string, Action<{{typeName}}>)[] TestMethods = new (string, Action<{{typeName}}>)[] {
{{ string.Join("," + Environment.NewLine,
    methodNames.Select(s => $"(\"{s}\", ({typeName} t) => t.{s}())")) }}
    };

    public static TestResult[] RunAllTests()
    {
        var results = new TestResult[TestMethods.Length];
        for (int i = 0; i < TestMethods.Length; i++)
        {
            var (name, a) = TestMethods[i];
            try
            {
                var t = new {{typeName}}();
                a(t);
                results[i] = new TestResult.Succeeded(name);
            }
            catch (Exception e)
            {
                results[i] = new TestResult.Failed(name, e);
            }
        }
        return results;
    }
}
""";
                prodCtx.AddSource($"{typeName}Methods", uxunitTestStruct);
            }
            var testMgrType = $$"""
namespace Uxunit;
internal static class TestManager
{
    public static void RunAllTests()
    {
{{ string.Join(
    Environment.NewLine,
    testCollection.MethodsByType.Select(item => $"{item.Key}UxunitTests.RunAllTests();")) }}
    }
}
""";
            prodCtx.AddSource($"TestManager", testMgrType);
        });

        static bool IsTestMethodFast(SyntaxNode node)
        {
            if (node is not MethodDeclarationSyntax { AttributeLists: { Count: > 0 } attrs })
            {
                return false;
            }
            return true;
        }

        static TestMethod? TryGetTestMethod(GeneratorSyntaxContext context)
        {
            var methodSymbol = (IMethodSymbol?)context.SemanticModel.GetDeclaredSymbol(context.Node);
            if (methodSymbol is null)
            {
                return null;
            }
            return new TestMethod(
                methodSymbol.ContainingType.ToDisplayString(),
                methodSymbol.Name,
                TestMethodType.Fact);
            //foreach (var attr in methodSymbol.GetAttributes())
            //{
            //    if (attr.AttributeClass?.IsWellKnown(WellKnownAttribute.Xunit_FactAttribute) == true)
            //    {
            //        return new TestMethod(
            //            methodSymbol.ContainingType.ToDisplayString(),
            //            methodSymbol.Name,
            //            TestMethodType.Fact);
            //    }
            //}
            //return null;
        }
    }

    private readonly record struct TestCollection(Dictionary<string, List<string>> MethodsByType);

    public readonly record struct TestMethod(
        string ContainingTypeName,
        string MethodName,
        TestMethodType Type);

    public enum TestMethodType
    {
        Fact,
        Theory
    }

    private static readonly ImmutableArray<string> s_attributeSimpleNames = ImmutableArray.Create("Fact", "Theory");
}