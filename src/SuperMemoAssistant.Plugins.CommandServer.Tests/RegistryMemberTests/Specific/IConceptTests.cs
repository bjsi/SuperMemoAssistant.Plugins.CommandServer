using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SuperMemoAssistant.Plugins.CommandServer.Tests.RegistryMemberTests
{
  public class IConceptTests : GenericRegMemberTestMethods<IConcept>
  {
    [Fact]
    public override void AddsAllMethods()
    {
      base.AddMethods();
    }
  }
}
