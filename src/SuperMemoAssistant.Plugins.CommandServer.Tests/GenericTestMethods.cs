using SuperMemoAssistant.Plugins.CommandServer.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SuperMemoAssistant.Plugins.CommandServer.Tests
{
  public abstract class GenericTestMethods<T>
  {

    public SvcCompiler Compiler { get; protected set; }
    public object InstantiatedObj { get; private set; }

    public void CreateInstance(params object[] activatorAgs)
    {
      // var results = Compiler.GenerateSourceCode();
      var compilerResults = Compiler.Compile();

      var type = compilerResults.CompiledAssembly.GetType($"{Shared.NameSpace}.{Shared.ClassName}");
      Assert.NotNull(type);

      var objec = Activator.CreateInstance(type, activatorAgs);
      Assert.NotNull(objec);

      InstantiatedObj = objec;
    }

    public virtual void AddMethods(params object[] activatorArgs)
    {
      Compiler.WithWrappedObjectField()
              .WithMethods();

      CreateInstance(activatorArgs);

      var methods = typeof(T)
        .GetMethods()
        .Where(x => x.DeclaringType == typeof(T))
        .Where(x => !x.IsSpecialName);

      foreach (var method in methods.Select(x => x.Name))
      {
        Assert.NotNull(InstantiatedObj.GetType().GetMethod(method));
      }
    }

    public virtual void AddsEvents(params object[] activatorArgs)
    {
      Compiler
        .WithWrappedObjectField()
        .WithEvents();

      CreateInstance(activatorArgs);

      var events = typeof(T).GetEvents().Select(x => x.Name);
      foreach (var e in events)
      {
        Assert.NotNull(InstantiatedObj.GetType().GetEvent(e));
      }
    }

    public virtual void AddPropertyMethods(params object[] activatorArgs)
    {

      Compiler
        .WithWrappedObjectField()
        .WithPropertiesAsMethods();

      CreateInstance(activatorArgs);

      var getProps = typeof(T).GetProperties().Where(x => x.CanRead);
      var setProps = typeof(T).GetProperties().Where(x => x.CanWrite);

      var objecType = InstantiatedObj.GetType();
      foreach (var get in getProps.Select(x => "Get" + x.Name))
      {
        var x = objecType
          .GetMethods()
          .Where(x => x.DeclaringType == objecType && !x.IsSpecialName)
          .Select(x => x.Name)
          .ToHashSet();

        Assert.Contains(get, x);
      }

      foreach (var set in setProps.Select(x => "Set" + x.Name))
      {
        var x = objecType
          .GetMethods()
          .Where(x => x.DeclaringType == objecType && !x.IsSpecialName)
          .Select(x => x.Name)
          .ToHashSet();

        Assert.Contains(set, x);
      }
    }
  }
}
