using System;
using System.Collections.Generic;
using System.Text;

namespace XunitGen;

public abstract partial record TestResult
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
    ) : TestResult(Test)
    {
        /// <summary>
        /// Gets the stack trace from <see cref="Exception" />, but with the 'internal' frames
        /// filtered out.
        /// </summary>
        public string? GetFilteredStackTrace()
        {
            var trace = Exception.StackTrace;
            if (trace is null)
            {
                return null;
            }
            var list = new List<string>();
            foreach (var line in trace.Split(Environment.NewLine, StringSplitOptions.None))
            {
                var trimmed = line.TrimStart();
                if (!trimmed.StartsWith("at Xunit.") && !trimmed.StartsWith("at XunitGen."))
                {
                    list.Add(line);
                }
            }
            return string.Join(Environment.NewLine, list);
        }
    }
    public sealed record Skipped(
        UnitTest Test
    ) : TestResult(Test);
}
