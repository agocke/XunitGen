
using Xunit;

// N.B. Namespace is not prefixed with "XunitGen" because that namespace gets filtered
// out of stack traces
namespace Tests.XunitGen;

public class BasicTest
{
    [Fact]
    public void Test1()
    {
        Assert.True(true);
    }

    [Fact]
    public void Test2()
    {
        Assert.True(false);
    }
}