using System;
using System.Collections.Generic;

namespace XunitGen;

public abstract record UnitTest
{
    public string Name { get; init; }
    public string TypeName { get; init; }

    private UnitTest(string name, string typeName)
    {
        Name = name;
        TypeName = typeName;
    }

    public abstract void RunTestCases(TestReporter results);

    public sealed record Fact<TTestClass>(
        string TestName,
        Action<TTestClass> Func
    ) : UnitTest(TestName, typeof(TTestClass).FullName!)
      where TTestClass : new()
    {
        public override void RunTestCases(TestReporter results)
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
            results.ReportResult(result);
        }
    }

    public sealed record Theory<TTestClass, T1>(
        string Name,
        Action<TTestClass, T1> Func,
        IEnumerable<T1> Data
    ) : UnitTest(Name, typeof(TTestClass).Name)
      where TTestClass : new()
    {
        public override void RunTestCases(TestReporter results)
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
                results.ReportResult(result);

            }
        }
    }

    public sealed record Theory<TTestClass, T1, T2>(
        string Name,
        Action<TTestClass, T1, T2> Func,
        IEnumerable<(T1, T2)> Data
    ) : UnitTest(Name, typeof(TTestClass).Name)
      where TTestClass : new()
    {
        public override void RunTestCases(TestReporter results)
        {
            foreach (var (p1, p2) in Data)
            {

            }
        }
    }
}