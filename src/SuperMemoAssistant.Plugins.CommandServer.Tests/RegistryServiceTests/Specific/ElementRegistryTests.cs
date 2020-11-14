using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.Elements;

namespace SuperMemoAssistant.Plugins.CommandServer.Tests.RegistryServiceTests
{
  public class ElementRegistryTests : GenericRegistryTestMethods<IElementRegistry, IElement>
  {
  }
}
