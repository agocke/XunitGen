using System;
using System.Collections.Generic;

namespace Uxunit;

public abstract record UnitTest
{
    public string Name { get; init; }

    private UnitTest(string name)
    {
        Name = name;
    }

    public abstract void RunTestCases(List<TestResult> results);

    public sealed record Fact<TTestClass>(
        string Name,
        Action<TTestClass> Func
    ) : UnitTest(Name)
      where TTestClass : new()
    {
        public override void RunTestCases(List<TestResult> results)
        {
            TestResult result;
            try
            {
                var t = new TTestClass();
                Func(t);
                result = new TestResult.Succeeded(this);
            }
            catch (Exception e)
            {
                result = new TestResult.Failed(this, e);
            }
            results.Add(result);
        }
    }

    public sealed record Theory<TTestClass, T1>(
        string Name,
        Action<TTestClass, T1> Func,
        IEnumerable<T1> Data
    ) : UnitTest(Name)
      where TTestClass : new()
    {
        public override void RunTestCases(List<TestResult> results)
        {
            foreach (var p1 in Data)
            {
                TestResult result;
                try
                {
                    var t = new TTestClass();
                    Func(t, p1);
                    result = new TestResult.Succeeded(this);
                }
                catch (Exception e)
                {
                    result = new TestResult.Failed(this, e);
                }
                results.Add(result);

            }
        }
    }

    public sealed record Theory<TTestClass, T1, T2>(
        string Name,
        Action<TTestClass, T1, T2> Func,
        IEnumerable<(T1, T2)> Data
    ) : UnitTest(Name)
      where TTestClass : new()
    {
        public override void RunTestCases(List<TestResult> results)
        {
            foreach (var (p1, p2) in Data)
            {

            }
        }
    }
}

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