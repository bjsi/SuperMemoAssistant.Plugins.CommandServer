using NSubstitute;
using SuperMemoAssistant.Interop.SuperMemo.Elements;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SuperMemoAssistant.Plugins.CommandServer.Tests.RegistryServiceTests
{
  public class Person
  {
    private void Hello() { }
  }

  public class James : Person
  {
    public void Wave() { }
  }

  public class GenericRegistryTestMethods<T, M> : GenericTestMethods<T>
    where T : class, IRegistry<M>
  {
    public T MockReg { get; } = Substitute.For<T>();

    public GenericRegistryTestMethods()
    {
      Compiler = Shared.CreateCompiler(MockReg);
    }

    [Fact]
    // TODO skipping the Add method because of out param
    public virtual void AddsAllMethods()
    {
      this.AddMethods(MockReg);
    }

    [Fact]
    public virtual void AddsAllPropertyMethods()
    {
      this.AddPropertyMethods(MockReg);
    }

    [Fact]
    public void AddsAllEvents()
    {
      this.AddsEvents(MockReg);
    }
  }
}
