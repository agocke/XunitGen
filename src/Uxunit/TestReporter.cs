
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Uxunit;

public sealed class TestReporter
{
    private int _passed = 0;
    private int _failed = 0;
    private readonly Stopwatch _timer = new Stopwatch();

    public void StartRun()
    {
        Console.WriteLine("Starting test execution, please wait...");
        _timer.Start();
    }

    public void ReportResult(TestResult r)
    {
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
                    var st = failure.Exception.StackTrace.Split('\n');
                    // Trim 2 frames off the end
                    var trimmed = st.Take(st.Length - 2);
                    Console.Error.WriteLine("  " + string.Join("\n", trimmed));
                    Console.Error.WriteLine();
                }
                finally
                {
                    Console.ResetColor();
                }
                break;
        }
    }

    public void PrintSummary()
    {
        _timer.Stop();
        string result = _failed != 0 ? "Failed!" : "Passed";
        Console.WriteLine($"{result} - Failed: {_failed}, Passed: {_passed}, Skipped: 0, Total: {_failed + _passed}, Duration: {_timer.ElapsedMilliseconds} ms");
    }
}