using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
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
    [Fact]
    public void Blah()
    {
      var b = new PythonClassBuilder<IElement>();
      var o = b.GenerateSourceCode();
    }
  }
}
