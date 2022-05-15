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
using Microsoft.CodeAnalysis.Diagnostics;

namespace XUnitWrapperGenerator;

[Generator]
public sealed class XUnitWrapperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methodsInSource = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => IsTestMethodFast(node),
                static (context, _) => (IMethodSymbol)context.SemanticModel.GetDeclaredSymbol(context.Node)!);

        static bool IsTestMethodFast(SyntaxNode node)
        {
            if (node is not MethodDeclarationSyntax { AttributeLists: { Count: > 0 } attrs })
            {
                return false;
            }
            return true;
        }
    }

    private static readonly ImmutableArray<string> s_attributeSimpleNames = ImmutableArray.Create("Fact", "Theory");
}