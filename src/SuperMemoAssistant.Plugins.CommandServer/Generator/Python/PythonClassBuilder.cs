using Stubble.Core.Builders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.CommandServer.Generator.Python
{
  public class PythonClassBuilder<T>
  {

    public const string BaseClassName = "SMA";
    public string ClassName { get; }
    public List<PythonClassMethod> Methods { get; } = new List<PythonClassMethod>();

    private Type Type { get; } = typeof(T);
    private string TemplatePath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Generator\Python\Templates");
    
    public PythonClassBuilder(string name = null)
    {
      ClassName = name ?? Type.Name;
    }

    public void WithMethods()
    {
      foreach (var m in Type.GetMethods().Where(x => !x.IsSpecialName))
      {
        var parameters = m.GetParameters();
        var pyParams = parameters.Select(x => new PythonParam(x));
        var name = m.Name;
        Methods.Add(new PythonClassMethod(name, pyParams, Array.Empty<string>(), true));
      }
    }

    public string GenerateSourceCode()
    {
      var classTemplate = Path.Combine(TemplatePath, "PythonClass.Mustache");
      var stubble = new StubbleBuilder().Build();
      using (StreamReader streamReader = new StreamReader(classTemplate, Encoding.UTF8))
      {
        return stubble.Render(streamReader.ReadToEnd(), this);
      }
    }
  }
}
