
using System;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace XunitGen;

partial class TestReporter
{
    public void WriteXmlOutput(string fileName)
    {
        using var writer = XmlWriter.Create(fileName, new XmlWriterSettings { Indent = true });
        writer.WriteStartElement("assemblies");
        {
            writer.WriteStartAttribute("timestamp");
            writer.WriteString(_started.ToString());
            writer.WriteEndAttribute();

            writer.WriteStartElement("assembly");
            {
                {
                    writer.WriteAttributeString("name", Environment.ProcessPath);
                    writer.WriteAttributeString("run-date", _started!.Value.ToString("yyyy-MM-dd"));
                    writer.WriteAttributeString("run-time", _started!.Value.ToString("HH:mm:ss"));
                    writer.WriteAttributeString("total", (_passed + _failed + _skipped).ToString());
                    writer.WriteAttributeString("passed", _passed.ToString());
                    writer.WriteAttributeString("failed", _failed.ToString());
                    writer.WriteAttributeString("skipped", _skipped.ToString());
                    writer.WriteAttributeString("time", _timer.Elapsed.TotalSeconds.ToString());
                    writer.WriteAttributeString("errors", "0");
                }

                writer.WriteElementString("errors", null);

                writer.WriteStartElement("collection");
                {
                    writer.WriteAttributeString("total", (_passed + _failed + _skipped).ToString());
                    writer.WriteAttributeString("passed", _passed.ToString());
                    writer.WriteAttributeString("failed", _failed.ToString());
                    writer.WriteAttributeString("skipped", _skipped.ToString());
                    writer.WriteAttributeString("name", "Test collection for ");
                    writer.WriteAttributeString("time", _timer.Elapsed.TotalSeconds.ToString());

                    foreach (var result in _results)
                    {
                        var test = result.Test;
                        writer.WriteStartElement("test");
                        {
                            writer.WriteAttributeString("name", test.TypeName + "." + test.Name);
                            writer.WriteAttributeString("type", test.TypeName);
                            writer.WriteAttributeString("method", test.Name);
                            switch (result)
                            {
                                case TestResult.Succeeded:
                                    writer.WriteAttributeString("result", "Pass");
                                    break;
                                case TestResult.Failed fail:
                                    writer.WriteAttributeString("result", "Fail");
                                    writer.WriteStartElement("failure");
                                    writer.WriteStartElement("message");
                                    writer.WriteValue(fail.Exception.Message);
                                    writer.WriteEndElement();
                                    writer.WriteStartElement("stack-trace");
                                    writer.WriteValue(fail.GetFilteredStackTrace());
                                    writer.WriteEndElement();
                                    break;
                            }
                            writer.WriteElementString("traits", null);
                        }
                        writer.WriteEndElement();
                    }
                }
            }
            writer.WriteEndElement();

        }
        writer.WriteEndElement();
        writer.Flush();
    }
}