namespace Uxunit;
internal static class BasicTestUxunitTests
{
    public static readonly (string, Action<BasicTest>)[] TestMethods = new (string, Action<BasicTest>)[] {
("Test1", (BasicTest t) => t.Test1()),
("Test2", (BasicTest t) => t.Test2())
    };

    public static TestResult[] RunAllTests()
    {
        var results = new TestResult[TestMethods.Length];
        for (int i = 0; i < TestMethods.Length; i++)
        {
            var (name, a) = TestMethods[i];
            try
            {
                var t = new BasicTest();
                a(t);
                results[i] = new TestResult.Succeeded(name);
            }
            catch (Exception e)
            {
                results[i] = new TestResult.Failed(name, e);
            }
        }
        return results;
    }
}