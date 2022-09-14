using System;

namespace Uxunit;

public abstract record TestResult
{
    public UnitTest Test { get; init; }

    private TestResult(UnitTest test)
    {
        Test = test;
    }

    public sealed record Succeeded(
        UnitTest Test
    ) : TestResult(Test);

    public sealed record Failed(
        UnitTest Test,
        Exception Exception
    ) : TestResult(Test);
}