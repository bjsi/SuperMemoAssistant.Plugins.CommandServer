using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SuperMemoAssistant.Plugins.CommandServer.Helpers
{
  public static class RefEx
  {
    public static List<MethodInfo> GetExtendedInterfaceMethods(this Type t)
    {
      var allMethods = new List<MethodInfo>();
      foreach (var iface in t.GetInterfaces())
      {
        foreach (var method in iface.GetMethods().Where(x => !x.IsSpecialName))
        {
          if (!allMethods.Any(x => x.Name == method.Name))
          {
            allMethods.Add(method);
          }
        }
      }

      return allMethods;
    }

    public static Type GetRegistryInterface(this Type type)
    {
      return type.GetInterface("IRegistry");
    }

    public static Type GetFirstGenericArg(this Type type)
    {
      return type.GetGenericArguments().First();
    }

    public static List<object> CreateUIList(ISuperMemoUI ui)
    {
      return ui.GetType().GetProperties().Select(x => x.GetValue(ui)).ToList();
    }

    public static IEnumerable<string> GetReferencedAssemblyPaths<T>()
    {
      return GetReferencedAssemblyPaths(typeof(T));
    }

    public static IEnumerable<string> GetReferencedAssemblyPaths(this Type type)
    {
      yield return type.Assembly.Location;

      foreach (AssemblyName assemblyName in type.Assembly.GetReferencedAssemblies())
      {
        yield return Assembly.ReflectionOnlyLoad(assemblyName.FullName).Location;
      }
    }

    public static Dictionary<Type, Type> CreateRegistryMap(ISuperMemoRegistry reg)
    {
      reg.ThrowIfArgumentNull("Failed to create registry map because ISuperMemoRegistry was null");

      var ret = new Dictionary<Type, Type>();
      foreach (var prop in typeof(ISuperMemoRegistry).GetProperties())
      {

        if (prop.PropertyType.Name.Contains("IComponentRegistry"))
          continue;

        var registry = prop.PropertyType;
        var regInterface = registry.GetInterfaces().Where(i => i.Name.StartsWith("IRegistry")).First();
        var regMember = regInterface.GenericTypeArguments.First();
        ret.Add(regMember, registry);
      }

      return ret;
    }

    // From: https://stackoverflow.com/questions/2448800/given-a-type-instance-how-to-get-generic-type-name-in-c
    // Other solutions available if this doesn't work
    public static string ToGenericTypeString(this Type t)
    {

      t.ThrowIfArgumentNull("Failed to convert to generic type string because object was null.");

      if (!t.IsGenericType)
        return t.Name;
      string genericTypeName = t.GetGenericTypeDefinition().Name;
      genericTypeName = genericTypeName.Substring(0,
          genericTypeName.IndexOf('`'));
      string genericArgs = string.Join(",",
          t.GetGenericArguments()
              .Select(ta => ToGenericTypeString(ta)).ToArray());
      return genericTypeName + "<" + genericArgs + ">";

    }

    /// <summary>
    /// Determine whether a type is simple
    /// or complex (i.e. custom class with public properties and methods).
    /// BUT excluding value types - enums and structs
    /// </summary>
    public static bool IsSimpleType(
      this Type type)
    {

      type.ThrowIfArgumentNull("Failed to check if type is simple type because type was null");

      return
        type.IsPrimitive ||
        new Type[] {
        typeof(String),
        typeof(Decimal),
				//typeof(DateTime),
				//typeof(DateTimeOffset),
				//typeof(TimeSpan),
				//typeof(Guid)
				}.Contains(type) ||
        Convert.GetTypeCode(type) != TypeCode.Object;
    }
  }
}
