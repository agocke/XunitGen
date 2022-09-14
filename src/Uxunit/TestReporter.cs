
using System;
using System.Collections.Generic;

namespace Uxunit;

public sealed class TestReporter
{
    private readonly List<TestResult> _results = new List<TestResult>();

    public void ReportResult(TestResult r)
    {
        _results.Add(r);
    }

    public void PrintSummary()
    {
        int passed = 0;
        int failed = 0;
        foreach (var result in _results)
        {
            switch (result)
            {
                case TestResult.Succeeded:
                    passed++;
                    break;
                case TestResult.Failed failedResult:
                    failed++;
                    Console.WriteLine(failedResult.Exception);
                    break;
            }
        }
        Console.WriteLine("Passed: " + passed);
        Console.WriteLine("Failed: " + failed);
    }
}