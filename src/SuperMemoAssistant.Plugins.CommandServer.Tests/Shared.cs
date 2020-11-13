using NSubstitute;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Plugins.CommandServer.Compiler;
using SuperMemoAssistant.Plugins.CommandServer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.CommandServer.Tests
{

  public static class Shared
  {
    public static string[] Imports = Array.Empty<string>();
    public static string ClassName { get; } = "TestClass";
    public static string NameSpace { get; } = "TestNamespace";
    private static ISuperMemoRegistry Registry { get; } = Substitute.For<ISuperMemoRegistry>();
    public static Dictionary<Type, object> RegistryMap = RefEx.CreateRegistryMap(Registry);

    //public static SvcCompiler CreateCompiler<T>()
    //{
    //  var refs = typeof(T).GetReferencedAssemblyPaths();
    //  return new SvcCompiler(ClassName,
    //                         NameSpace,
    //                         Imports,
    //                         refs,
    //                         null,
    //                         typeof(T),
    //                         RegistryMap);
    //}

    public static SvcCompiler CreateCompiler<T>(T obj)
    {
      var refs = typeof(T).GetReferencedAssemblyPaths();
      return new SvcCompiler(ClassName,
                             NameSpace,
                             Imports,
                             refs,
                             obj,
                             typeof(T),
                             RegistryMap);
    }
  }
}
