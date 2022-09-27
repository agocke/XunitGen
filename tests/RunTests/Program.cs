// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Tests.XunitGen;
using Xunit;

var tfmDir = Directory.GetParent(Environment.ProcessPath!)!;
var configDir = tfmDir.Parent!;
var binDir = configDir.Parent!.Parent!;
var testsDir = Path.Combine(binDir.Parent!.FullName, "tests");
Directory.CreateDirectory(testsDir);


var basicTestsDir = Path.Combine(binDir.FullName, "BasicTests", configDir.Name, tfmDir.Name);
var exeSuffix = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "exe" : null;

var basicTestsExe = Path.ChangeExtension(Path.Combine(basicTestsDir, "BasicTests"), exeSuffix);
var actualXml = Path.Combine(testsDir, "actualBasicTests.xml");
var proc = Process.Start(new ProcessStartInfo
{
    FileName = basicTestsExe,
    Arguments = $"-xml {actualXml}",
    RedirectStandardOutput = true,
    RedirectStandardError = true
});
proc!.WaitForExit();

var expectedXml = Path.Combine(testsDir, "expectedBasicTests.xml");
proc = Process.Start(new ProcessStartInfo
{
    FileName = "dotnet",
    Arguments = $"test --logger:xunit;LogFilePath={expectedXml}",
    WorkingDirectory = Path.Combine(binDir.Parent!.Parent!.FullName, "tests/BasicTests"),
    RedirectStandardOutput = true,
    RedirectStandardError = true
});
proc!.WaitForExit();

var actualResult = ReadTestResults(actualXml);
var expectedResult = ReadTestResults(expectedXml);

Array.Sort(actualResult.Assemblies[0].Collection[0].Tests);
Array.Sort(expectedResult.Assemblies[0].Collection[0].Tests);

Console.WriteLine("Checking test output equality: ");
for (int i = 0; i < actualResult.Assemblies[0].Collection[0].Tests.Length; i++)
{
    var test = actualResult.Assemblies[0].Collection[0].Tests[i];
    var test2 = expectedResult.Assemblies[0].Collection[0].Tests[i];
    Assert.Equal(test, test2);
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("All tests passed.");

static TestRun ReadTestResults(string path)
{
    var serializer = new XmlSerializer(typeof(TestRun), new XmlRootAttribute("assemblies"));
    using var stream = File.OpenRead(path);
    var result = (TestRun)serializer.Deserialize(stream)!;
    return result;
}