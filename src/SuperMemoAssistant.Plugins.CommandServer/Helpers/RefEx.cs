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

    public static Dictionary<Type, object> CreateRegistryMap(ISuperMemoRegistry reg)
    {
      reg.ThrowIfArgumentNull("Failed to create registry map because ISuperMemoRegistry was null");

      var ret = new Dictionary<Type, object>();
      foreach (var prop in typeof(ISuperMemoRegistry).GetProperties())
      {

        // TODO: IComponentRegistry doesn't implement IRegistry
        if (prop.PropertyType.Name.Contains("IComponentRegistry"))
          continue;

        var i = prop.PropertyType.GetInterfaces().Where(i => i.Name.StartsWith("IRegistry")).First();
        var regMember = i.GenericTypeArguments.First();
        var obj = prop.GetValue(reg);
        ret.Add(regMember, obj);
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
    /// </summary>
    /// <see cref="http://stackoverflow.com/questions/2442534/how-to-test-if-type-is-primitive"/>
    public static bool IsSimpleType(
      this Type type)
    {

      type.ThrowIfArgumentNull("Failed to check if type is simple type because type was null");

      return
        type.IsValueType ||
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
