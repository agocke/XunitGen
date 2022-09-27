
using System.Xml.Serialization;

namespace Tests.XunitGen;

static class HashHelpers
{
    public static int GetHashCode<T>(ReadOnlySpan<T> span)
    {
        int code = 0;
        foreach (var t in span)
        {
            code = HashCode.Combine(code, t);
        }
        return code;
    }
}

[XmlRoot("assemblies")]
public partial record struct TestRun : IEquatable<TestRun>
{
    [XmlElement("assembly")]
    public Assembly[] Assemblies { get; init; }

    public bool Equals(TestRun other)
        => MemoryExtensions.SequenceEqual<Assembly>(Assemblies, other.Assemblies);

    public override int GetHashCode() => HashHelpers.GetHashCode<Assembly>(Assemblies);
}

public partial record struct Assembly : IEquatable<Assembly>
{
    [XmlElement("collection")]
    public Collection[] Collection { get; init; }

    public bool Equals(Assembly other)
        => MemoryExtensions.SequenceEqual<Collection>(Collection, other.Collection);

    public override int GetHashCode() => HashHelpers.GetHashCode<Collection>(Collection);
}

public partial record struct Collection
{
    [XmlElement("test")]
    public Test[] Tests { get; init; }

    public bool Equals(Collection other)
        => MemoryExtensions.SequenceEqual<Test>(Tests, other.Tests);

    public override int GetHashCode() => HashHelpers.GetHashCode<Test>(Tests);
}

public partial record Test : IComparable<Test>
{
    [XmlAttribute("name")]
    public string Name { get; init; } = null!;

    [XmlAttribute("result")]
    public TestResult Result { get; init; }

    [XmlElement("failure")]
    public Failure? Failure { get; init; }

    public int CompareTo(Test? other)
    {
        return Name.CompareTo(other?.Name);
    }
}

public partial record struct Failure : IEquatable<Failure>
{
    [XmlElement("message")]
    public string Message { get; init; }
    [XmlElement("stack-trace")]
    public string StackTrace { get; init; }

    public bool Equals(Failure other)
    {
        return Message == other.Message;
    }
    public override int GetHashCode() => Message.GetHashCode();
}

public enum TestResult
{
    Pass,
    Fail,
    Skip
}