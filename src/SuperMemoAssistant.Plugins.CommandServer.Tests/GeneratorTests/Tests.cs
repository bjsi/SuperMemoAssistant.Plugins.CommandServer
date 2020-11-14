using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.UI.Element;
using SuperMemoAssistant.Plugins.CommandServer.Generator;
using SuperMemoAssistant.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SuperMemoAssistant.Plugins.CommandServer.Tests.GeneratorTests
{
  public class Tests
  {

    [Fact]
    public void Blah()
    {
      var sm = typeof(IControlHtml);
      var a = Assembly.GetAssembly(sm);
      var r = new Reflector(a);
      var g = r.GetDependencyGraph();
      var e = g.EdgesFor(sm);
    }
  }
}
