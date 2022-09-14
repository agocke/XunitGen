namespace Uxunit;
internal static class TestManager
{
    public static TestReporter RunAllTests()
    {
        var reporter = new TestReporter();
        BasicTestUxunitTests.RunAllTests(reporter);
        return reporter;
    }
    public static void Main()
    {
        var reporter = RunAllTests();
        reporter.PrintSummary();
    }
}