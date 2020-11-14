using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Plugins.CommandServer.Compiler;
using SuperMemoAssistant.Plugins.CommandServer.Helpers;
using System;
using Xunit;
using NSubstitute;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Types;

namespace SuperMemoAssistant.Plugins.CommandServer.Tests
{

  public class RegistryMemberSvcTests
  {

    private IElement Element { get; } = Substitute.For<IElement>();
    private IRegistry<IElement> ElementRegistry { get; } = Substitute.For<IRegistry<IElement>>();

    // TODO: Events do not work on registry members like IElement yet.
    [Fact]
    public void ConvertsActionEvents()
    {

      var refs = RefEx.GetReferencedAssemblyPaths<IElement>();
      var compiler = new SvcCompiler(Shared.ClassName,
                                     Shared.NameSpace,
                                     Shared.Imports,
                                     refs,
                                     Element,
                                     typeof(IElement),
                                     Shared.RegMemberToRegTypeMap);

      compiler.WithWrappedObjectField()
              .WithEvents();

      var compilerResults = compiler.Compile();

      var type = compilerResults.CompiledAssembly.GetType($"{Shared.NameSpace}.{Shared.ClassName}");
      Assert.NotNull(type);

      var objec = Activator.CreateInstance(type);
      Assert.NotNull(objec);

      // TODO: not supported yet
      var events = type.GetEvents();
      Assert.Empty(events);

    }

    [Fact]
    public void CompilerAddsMethods()
    {
      var refs = RefEx.GetReferencedAssemblyPaths<IElement>();
      var compiler = new SvcCompiler(Shared.ClassName,
                                     Shared.NameSpace,
                                     Shared.Imports,
                                     refs,
                                     Element,
                                     typeof(IElement),
                                     Shared.RegMemberToRegTypeMap);

      compiler.WithWrappedObjectField()
              .WithMethods();

      var compilerResults = compiler.Compile();
      var type = compilerResults.CompiledAssembly.GetType($"{Shared.NameSpace}.{Shared.ClassName}");
      Assert.NotNull(type);

      var objec = Activator.CreateInstance(type, new object[] { ElementRegistry });
      Assert.NotNull(objec);

      var delete = type.GetMethod("Delete");
      var done = type.GetMethod("Done");
      var move = type.GetMethod("MoveTo");
      var display = type.GetMethod("Display");

      Assert.NotNull(delete);
      Assert.NotNull(done);
      Assert.NotNull(move);
      Assert.NotNull(display);

    }

    [Fact]
    public void CompilerConvertsPropertiesToMethods()
    {
      var refs = RefEx.GetReferencedAssemblyPaths<IElement>();
      var compiler = new SvcCompiler(Shared.ClassName,
                                     Shared.NameSpace,
                                     Shared.Imports,
                                     refs,
                                     Element,
                                     typeof(IElement),
                                     Shared.RegMemberToRegTypeMap);

      compiler.WithWrappedObjectField()
              .WithPropertiesAsMethods();

      var compilerResults = compiler.Compile();

      var type = compilerResults.CompiledAssembly.GetType($"{Shared.NameSpace}.{Shared.ClassName}");
      Assert.NotNull(type);

      dynamic objec = Activator.CreateInstance(type, ElementRegistry );
      Assert.NotNull(objec);

      var methods = type.GetMethods();

      // TODO assertions

    }
  }
}
