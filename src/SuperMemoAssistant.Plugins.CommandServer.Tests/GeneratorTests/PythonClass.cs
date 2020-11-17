using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Plugins.CommandServer.Generator;
using SuperMemoAssistant.Plugins.CommandServer.Generator.Python;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SuperMemoAssistant.Plugins.CommandServer.Tests.GeneratorTests
{
  public class PythonClass
  {

    private HashSet<Type> PublicFacingTypes = new SMATypeGenerator(Shared.SuperMemoRegistry).GetPublicFacingTypes(true);

    [Fact]
    public void TestProperties()
    {
      var b = new PythonClassBuilder(typeof(IElement), PublicFacingTypes);
      b.WithProperties();
      var o = b.GenerateSourceCode();
    }

    [Fact]
    public void TestMethods()
    {
      var b = new PythonClassBuilder(typeof(IElement), PublicFacingTypes);
      b.WithMethods();
      var o = b.GenerateSourceCode();
    }
  }
}
