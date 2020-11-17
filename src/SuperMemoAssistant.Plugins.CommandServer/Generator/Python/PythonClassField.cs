using Stubble.Core.Builders;
using System;
using System.IO;
using System.Text;

namespace SuperMemoAssistant.Plugins.CommandServer.Generator.Python
{
  public class PythonClassField : PythonSourceGenerator
  {

    public string Name { get; }
    public string PyType { get; }
    public string PropertyString => Generate("PythonClassField.Mustache");
    private string TemplatePath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Generator\Python\Templates");

    public PythonClassField(string name, string pyType)
    {
      Name = name;
      PyType = pyType ?? "Any"; // If the type is unknown, put Any
    }
  }
}
