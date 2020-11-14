using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Plugins.CommandServer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.CommandServer.Generator.Python
{
  public class TypeGenerator<T>
  {
    private Type Type { get; } = typeof(T);

    private HashSet<Type> GetMethodTypes(IEnumerable<MethodInfo> methods)
    {
      var ret = new HashSet<Type>();

      // get return types and parameter types where the type is not a simple type
      foreach (var m in methods)
      {
        // return type
        var retType = m.ReturnType;
        // parameters
        // TODO: skip out param type because it gets messed up
        var methodTypes = m.GetParameters()
                           .Where(x => !x.IsOut)
                           .Select(x => x.ParameterType)
                           .ToHashSet();

        ret.UnionWith(methodTypes);
      }
      return ret;
    }

    private HashSet<Type> FilterTypes(IEnumerable<Type> types)
    {
      var ret = new HashSet<Type>();

      foreach (var t in types)
      {
        if (!t.IsSimpleType())
        {
          // TODO: IControl, IControlGroup, IComponent, IComponentGroup and Control/Component Registries are messed up
          if (t == typeof(IControlGroup)) // IControlGroup inherits IEnumerable and IControl
            ret.Add(t);
          else if (t.IsGenericType)
            ret.UnionWith(UnwrapGenericParams(t));

          else if (IsIEnumerableType(t))
            ret.UnionWith(GetIEnumerableTypeArgs(t));

          else
            ret.Add(t);
        }
      }

      return ret;
    }

    public HashSet<Type> GetPublicFacingTypes(bool includeProperties = false)
    {
      var type = typeof(T);
      var ret = new HashSet<Type>();

      foreach (var p in type.GetProperties())
      {
        // get type
        var t = p.PropertyType;
        var allTypes = new HashSet<Type>();

        var methods = t.GetMethods().Where(x => !x.IsSpecialName).ToList();
        if (t.IsInterface)
          methods.AddRange(t.GetExtendedInterfaceMethods());

        // Do properties for tests
        if (includeProperties)
        {
          var props = t.GetProperties()
                       .Where(x => !x.IsSpecialName)
                       .Select(x => x.PropertyType)
                       .Where(x => !x.IsSimpleType());

          allTypes.UnionWith(props);
        }

        // get methods, incl. interface extensions
        var methodTypes = GetMethodTypes(methods);

        var eventTypes = t.GetEvents()
                          .Select(x => x.EventHandlerType)
                          .Where(x => !x.IsSimpleType())
                          .ToHashSet();

        allTypes.UnionWith(eventTypes);
        allTypes.UnionWith(methodTypes);
        ret.UnionWith(FilterTypes(allTypes));

      }

      return ret;
    }

    // Could be IEnumerable without T param
    private bool IsIEnumerableType(Type type)
    {
      var iFaces = type.GetInterfaces();
      foreach (Type interfaceType in iFaces)
      {
        if (interfaceType.IsGenericType &&
            interfaceType.GetGenericTypeDefinition()
            == typeof(IEnumerable<>))
        {
          return true;
        }
      }
      return false;
    }

    private HashSet<Type> GetIEnumerableTypeArgs(Type type)
    {
      foreach (Type interfaceType in type.GetInterfaces())
      {
        if (interfaceType.IsGenericType &&
            interfaceType.GetGenericTypeDefinition()
            == typeof(IEnumerable<>))
        {
          return type.GetGenericArguments().ToHashSet();
        }
      }
      return new HashSet<Type>();
    }

    private HashSet<Type> UnwrapGenericParams(Type t)
    {
      var ret = new HashSet<Type>();
      var parameters = t.GetGenericArguments();
      foreach (var p in parameters)
      {
        if (p.IsGenericType)
        {
          ret.UnionWith(UnwrapGenericParams(p));
        }
        else
        {
          ret.Add(p);
        }
      }

      return ret;
    }
  }
}
