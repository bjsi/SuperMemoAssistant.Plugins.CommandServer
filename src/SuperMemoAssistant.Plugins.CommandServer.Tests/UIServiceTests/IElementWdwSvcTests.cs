using SuperMemoAssistant.Interop.SuperMemo.UI.Element;
using SuperMemoAssistant.Plugins.CommandServer.Compiler;
using SuperMemoAssistant.Plugins.CommandServer.Helpers;
using System;
using Xunit;
using SuperMemoAssistant.Interop.SuperMemo;
using NSubstitute;
using System.Linq;

namespace SuperMemoAssistant.Plugins.CommandServer.Tests
{
  public class IElementWdwSvcTests
  {

    private static IElementWdw ElementWdw { get; } = Substitute.For<IElementWdw>();
    private SvcCompiler compiler { get; } = Shared.CreateCompiler(ElementWdw);

    [Fact]
    public void SuccessfullyAddsEvents()
    {
      compiler
        .WithWrappedObjectField()
        .WithEvents();

      var compilerResults = compiler.Compile();

      var type = compilerResults.CompiledAssembly.GetType($"{Shared.NameSpace}.{Shared.ClassName}");
      Assert.NotNull(type);

      var objec = Activator.CreateInstance(type, ElementWdw);
      Assert.NotNull(objec);

      foreach (var e in typeof(IElementWdw).GetEvents().Select(x => x.Name))
      {
        Assert.NotNull(objec.GetType().GetEvent(e));
      }

    }

    [Fact]
    public void SuccessfullyConvertsPropertiesToMethods()
    {
      compiler
        .WithWrappedObjectField()
        .WithPropertiesAsMethods();

      var compilerResults = compiler.Compile();

      var type = compilerResults.CompiledAssembly.GetType($"{Shared.NameSpace}.{Shared.ClassName}");
      Assert.NotNull(type);

      var objec = Activator.CreateInstance(type, ElementWdw);
      Assert.NotNull(objec);

      var getProps = typeof(IElementWdw).GetProperties().Where(x => x.CanRead);
      var setProps = typeof(IElementWdw).GetProperties().Where(x => x.CanWrite);

      var objecType = objec.GetType();
      foreach (var get in getProps.Select(x => "Get" + x.Name))
      {
        Assert.NotNull(objecType.GetMethod(get));
      }

      foreach (var set in setProps.Select(x => "Set" + x.Name))
      {
        Assert.NotNull(objecType.GetMethod(set));
      }

    }

    [Fact]
    public void SuccessfullyAddsAllMethods()
    {
      compiler
        .WithWrappedObjectField()
        .WithMethods();

      var compilerResults = compiler.Compile();

      var type = compilerResults.CompiledAssembly.GetType($"{Shared.NameSpace}.{Shared.ClassName}");
      Assert.NotNull(type);

      var objec = Activator.CreateInstance(type, ElementWdw);
      Assert.NotNull(objec);

      var methods = typeof(IElementWdw).GetMethods()
                                       .Where(x => x.DeclaringType == typeof(IElementWdw))
                                       .Where(x => !x.IsSpecialName);

      foreach (var method in methods.Select(x => x.Name))
      {
        Assert.NotNull(objec.GetType().GetMethod(method));
      }

    }

    [Fact]
    public void SuccessfullyCreatesElementWdwSvc()
    {
      compiler.WithAllAttributes();

      var compilerResults = compiler.Compile();

      var type = compilerResults.CompiledAssembly.GetType($"{Shared.NameSpace}.{Shared.ClassName}");
      Assert.NotNull(type);

      var objec = Activator.CreateInstance(type, ElementWdw);
      Assert.NotNull(objec);

    }
  }
}
