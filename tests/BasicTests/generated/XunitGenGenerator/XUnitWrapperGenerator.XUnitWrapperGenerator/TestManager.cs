using System;

namespace XunitGen;
internal static class TestManager
{
    public static TestReporter RunAllTests()
    {
        var reporter = new TestReporter();
        BasicTestXunitGenTests.RunAllTests(reporter);
        return reporter;
    }
    public static void Main(string[] args)
    {
        string? xmlPath = null;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-xml")
            {
                i++;
                xmlPath = args[i];
            }
            else
            {
                throw new ArgumentException("Invalid argument" + args[i]);
            }
        }
        var reporter = RunAllTests();
        reporter.PrintSummary();
        if (xmlPath is not null)
        {
            reporter.WriteXmlOutput(xmlPath);
        }
    }
}