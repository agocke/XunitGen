﻿using Tests.XunitGen;

namespace XunitGen;
internal static class BasicTestXunitGenTests
{
    public static readonly UnitTest[] TestMethods = new[] {
new UnitTest.Fact<BasicTest>("Test1", (Tests.XunitGen.BasicTest t) => t.Test1()),
new UnitTest.Fact<BasicTest>("Test2", (Tests.XunitGen.BasicTest t) => t.Test2())
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