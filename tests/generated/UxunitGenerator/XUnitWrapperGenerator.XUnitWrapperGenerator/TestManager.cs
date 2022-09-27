namespace XunitGen;
internal static class TestManager
{
    public static TestReporter RunAllTests()
    {
        var reporter = new TestReporter();
        BasicTestXunitGenTests.RunAllTests(reporter);
        return reporter;
    }
    public static void Main()
    {
        var reporter = RunAllTests();
        reporter.PrintSummary();
    }
}