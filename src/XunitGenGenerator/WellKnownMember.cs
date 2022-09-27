
using System;
using Microsoft.CodeAnalysis;

internal enum WellKnownAttribute
{
    Xunit_FactAttribute,
    Xunit_TheoryAttribute,

    Count
}

internal static class WellKnownAttributes
{
    private static readonly string[] WkAttributeNames = new[]
    {
        "Xunit.FactAttribute",
        "Xunit.TheoryAttribute"
    };
    public static string GetFqn(this WellKnownAttribute wk) => WkAttributeNames[(int)wk];
}

internal static class AttributeExtensions
{
    public static bool IsWellKnown(this INamedTypeSymbol type, WellKnownAttribute wk)
        => type.ToDisplayString() == wk.GetFqn();
}