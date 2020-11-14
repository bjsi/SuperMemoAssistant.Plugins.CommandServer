using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Plugins.CommandServer.Compiler;
using SuperMemoAssistant.Plugins.CommandServer.Helpers;
using SuperMemoAssistant.Services;
using System;
using System.Collections.Generic;

namespace SuperMemoAssistant.Plugins.CommandServer.Services
{
  public class SvcTypeFactory
  {
    private const string NameSpace = "CompiledServices";
    private Dictionary<Type, Type> RegMemberToRegTypeMap { get; } = RefEx.CreateRegistryMap(Svc.SM.Registry);
    private object Object { get; }
    private Type ObjectType { get; }
    private string ClassName { get; }

    // TODO: object can be null
    public SvcTypeFactory(object targetObject, Type type)
    {
      type.ThrowIfArgumentNull("Failed to create service type factory because type was null");
      this.Object = targetObject;
      this.ObjectType = type;
      this.ClassName = this.ObjectType.Name + "Svc";
    }

    public Type Compile()
    {
      var refs = ObjectType.GetReferencedAssemblyPaths();
      var compiler = new SvcCompiler(ClassName, NameSpace, Array.Empty<string>(), refs, Object, ObjectType, RegMemberToRegTypeMap);
      var compilerResults = compiler.WithAllAttributes().Compile();
      return compilerResults.CompiledAssembly.GetType($"{NameSpace}.{ClassName}");
    }
  }
}
