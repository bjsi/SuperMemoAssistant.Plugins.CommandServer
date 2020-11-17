using Stubble.Core.Builders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.CommandServer.Generator.Python
{
  public abstract class PythonSourceGenerator
  {

    private string TemplatePath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Generator\Python\Templates");
    protected string Generate(string templateName)
    {
      var classTemplate = Path.Combine(TemplatePath, templateName);
      var stubble = new StubbleBuilder().Build();
      using (StreamReader streamReader = new StreamReader(classTemplate, Encoding.UTF8))
      {
        return stubble.Render(streamReader.ReadToEnd(), this, new Stubble.Core.Settings.RenderSettings { SkipHtmlEncoding = true });
      }
    }
  }
}
