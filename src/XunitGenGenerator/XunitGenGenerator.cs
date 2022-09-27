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
                var methodsByType = new Dictionary<TypeName, List<string>>();
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
                var XunitGenTestStruct = $$"""
using {{typeName.Namespace}};

namespace XunitGen;
internal static class {{typeName.Name}}XunitGenTests
{
    public static readonly UnitTest[] TestMethods = new[] {
{{ string.Join("," + Environment.NewLine,
    methodNames.Select(s => $"""
new UnitTest.Fact<BasicTest>("{s}", ({typeName} t) => t.{s}())
"""))
}}
    };

    public static void RunAllTests(TestReporter reporter)
    {
        reporter.StartRun();
        foreach (var testMethod in TestMethods)
        {
            testMethod.RunTestCases(reporter);
        }
        reporter.EndRun();
    }
}
""";
                prodCtx.AddSource($"{typeName}Methods", XunitGenTestStruct);
            }
            var resultsStmts = testCollection.MethodsByType.Select(
                item => $"{item.Key.Name}XunitGenTests.RunAllTests(reporter);").ToList();
            var testMgrType = $$"""
using System;

namespace XunitGen;
internal static class TestManager
{
    public static TestReporter RunAllTests()
    {
        var reporter = new TestReporter();
        {{ string.Join(Environment.NewLine, resultsStmts) }}
        return reporter;
    }
    public static void Main(string[] args)
    {
        string? xmlPath = null;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-xml")
            {
                i++;
                xmlPath = args[i];
            }
            else
            {
                throw new ArgumentException("Invalid argument" + args[i]);
            }
        }
        var reporter = RunAllTests();
        reporter.PrintSummary();
        if (xmlPath is not null)
        {
            reporter.WriteXmlOutput(xmlPath);
        }
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
            var containingType = methodSymbol.ContainingType;
            return new TestMethod(
                new(containingType.ContainingNamespace.ToDisplayString(), containingType.Name),
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

    public record struct TypeName(string Namespace, string Name)
    {
        public override string ToString() => FullName;
        public string FullName => Namespace + "." + Name;
    }

    private readonly record struct TestCollection(Dictionary<TypeName, List<string>> MethodsByType);

    public readonly record struct TestMethod(
        TypeName ContainingTypeName,
        string MethodName,
        TestMethodType Type);

    public enum TestMethodType
    {
        Fact,
        Theory
    }

    private static readonly ImmutableArray<string> s_attributeSimpleNames = ImmutableArray.Create("Fact", "Theory");
}