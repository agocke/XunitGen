
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace XunitGen;

public sealed partial class TestReporter
{
    private int _passed = 0;
    private int _failed = 0;
    private int _skipped = 0;
    private readonly Stopwatch _timer = new Stopwatch();
    private DateTime? _started = null;
    private readonly List<TestResult> _results = new();

    public void StartRun()
    {
        Console.WriteLine("Starting test execution, please wait...");
        _timer.Start();
        _started = DateTime.Now;
    }

    public void ReportResult(TestResult r)
    {
        lock(_results)
        {
            _results.Add(r);
            switch (r)
            {
                case TestResult.Succeeded:
                    _passed++;
                    break;
                case TestResult.Failed failure:
                    _failed++;
                    try
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Error.WriteLine($"[xUnit {_timer.Elapsed}] {failure.Test.Name} [FAIL]");
                        Console.Error.WriteLine("Error Message:");
                        Console.Error.WriteLine("  " + failure.Exception.Message);
                        Console.Error.WriteLine("Stack trace:");
                        Console.Error.WriteLine("  " + failure.GetFilteredStackTrace());
                        Console.Error.WriteLine();
                    }
                    finally
                    {
                        Console.ResetColor();
                    }
                    break;
            }
        }
    }

    public void EndRun()
    {
        _timer.Stop();
    }

    public void PrintSummary()
    {
        string result = _failed != 0 ? "Failed!" : "Passed";
        Console.WriteLine($"{result} - Failed: {_failed}, Passed: {_passed}, Skipped: 0, Total: {_failed + _passed}, Duration: {_timer.ElapsedMilliseconds} ms");
    }
}