
using Microsoft.CodeAnalysis;

internal readonly record struct TestMethod(
    IMethodSymbol TargetMethod
);