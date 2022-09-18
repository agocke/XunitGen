
using System;
using System.Diagnostics;
using System.Xml.Serialization;
using Serde;

namespace Uxunit;

partial class TestReporter
{
    private readonly record struct RunDateWrap(DateTime Value) : ISerialize
    {
        void ISerialize.Serialize(ISerializer serializer) => serializer.SerializeString(Value.ToString("yyyy-MM-dd"));
    }

    private readonly record struct TimeSpanWrap(TimeSpan Value) : ISerialize
    {
        void ISerialize.Serialize(ISerializer serializer) => serializer.SerializeDouble(Value.TotalSeconds);
    }


    [GenerateSerialize]
    [SerdeTypeOptions(MemberFormat = MemberFormat.CamelCase)]
    private readonly partial record struct AssemblyResult(
        /// <summary>The fully qualified path name of the test assembly.</summary>
        [property: XmlAttribute]
        [property: SerdeMemberOptions(ProvideAttributes = true)]
        string Name,
        /// <summary>The date when the test run started, in yyyy-mm-dd format.</summary>
        [property: SerdeWrap(typeof(RunDateWrap))]
        [property: XmlAttribute]
        [property: SerdeMemberOptions(ProvideAttributes = true)]
        DateTime RunDate,
        /// <summary>The number of seconds the assembly run took, in decimal format.</summary>
        [property: SerdeWrap(typeof(TimeSpanWrap))]
        [property: XmlAttribute]
        [property: SerdeMemberOptions(ProvideAttributes = true)]
        TimeSpan Time,
        /// <summary>The total number of test cases in the assembly which passed.</summary>
        [property: XmlAttribute]
        [property: SerdeMemberOptions(ProvideAttributes = true)]
        int Passed,
        /// <summary>The total number of test cases in the assembly which failed.</summary>
        [property: XmlAttribute]
        [property: SerdeMemberOptions(ProvideAttributes = true)]
        int Failed,
        /// <summary>The total number of test cases in the assembly that were skipped.</summary>
        [property: XmlAttribute]
        [property: SerdeMemberOptions(ProvideAttributes = true)]
        int Skipped,
        /// <summary>The total number of environmental errors experienced in the assembly.</summary>
        [property: XmlAttribute]
        [property: SerdeMemberOptions(ProvideAttributes = true)]
        int Errors = 0) : ISerialize
    {
        /// <summary>The total number of test cases run in the assembly.</summary>
        [property: XmlAttribute]
        [property: SerdeMemberOptions(ProvideAttributes = true)]
        public int Total => Passed + Failed + Skipped;

        /// <summary>The fully qualified path name of the test assembly configuration file.</summary>
        [property: XmlAttribute]
        [property: SerdeMemberOptions(ProvideAttributes = true)]
        public string? ConfigFile { get; init; }
        /// <summary>The display name of the test framework that ran the tests.</summary>
        [property: XmlAttribute]
        [property: SerdeMemberOptions(ProvideAttributes = true)]
        public string? TestFramework { get; init; }
        /// <summary>The runtime environment in which the tests were run.</summary>
        [property: XmlAttribute]
        [property: SerdeMemberOptions(ProvideAttributes = true)]
        public string? Environment { get; init; }
    }

    [GenerateSerialize]
    [SerdeTypeOptions(MemberFormat = MemberFormat.CamelCase)]
    private partial record struct AssembliesResult(AssemblyResult Assembly);

    public string GetXmlOutput()
    {
        return Serde.XmlSerializer.Serialize(new AssembliesResult(new AssemblyResult(
            Name: Environment.ProcessPath ?? "",
            RunDate: _started!.Value,
            Time: _timer.Elapsed,
            _passed,
            _failed,
            _skipped
        )));
    }
}