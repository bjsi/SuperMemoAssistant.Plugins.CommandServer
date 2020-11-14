using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.UI.Element;
using SuperMemoAssistant.Plugins.CommandServer.Generator;
using SuperMemoAssistant.Plugins.CommandServer.Generator.DependencyGraph;
using SuperMemoAssistant.Plugins.CommandServer.Generator.Python;
using SuperMemoAssistant.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace SuperMemoAssistant.Plugins.CommandServer.Tests.GeneratorTests
{
  public class TypeGeneratorTests
  {
    [Fact]
    public void TestRegistryMemberType()
    {
      var g = new TypeGenerator<IElement>();
      var t = g.GetPublicFacingTypes(true);
    }

    [Fact]
    public void TestRegistryTypes()
    {
      var g = new TypeGenerator<ISuperMemoRegistry>();
      var t = g.GetPublicFacingTypes(true);
    }

    [Fact]
    public void TestUITypes()
    {
      var g = new TypeGenerator<ISuperMemoUI>();
      var t = g.GetPublicFacingTypes(true);
    }
  }
}
