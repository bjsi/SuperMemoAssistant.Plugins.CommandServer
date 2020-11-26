using NSubstitute;
using System;
using Xunit;

namespace SuperMemoAssistant.Plugins.CommandServer.Tests.RegistryMemberTests
{
  public abstract class GenericRegMemberTestMethods<T> : GenericTestMethods<T> where T : class
  {
    public T MockRegMember { get; } = Substitute.For<T>();
    public object MockReg { get; }

    public GenericRegMemberTestMethods()
    {
      Compiler = Shared.CreateCompiler<T>(null);
      var regType = Shared.RegMemberToRegTypeMap[typeof(T)];
      MockReg = Substitute.For(new Type[] { regType }, null);
    }

    [Fact]
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
    // TODO: Events not supported on reg members yet
    public void AddsAllEvents()
    {
      CreateInstance();
      var events = InstantiatedObj.GetType().GetEvents();
      Assert.Empty(events);
    }
  }
}
