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

    private HashSet<Type> GetEventTypes(IEnumerable<EventInfo> events)
    {
      return events.Select(x => x.EventHandlerType)
                   .Where(x => !x.IsSimpleType())
                   .ToHashSet();
    }

    private HashSet<Type> GetPropertyTypes(IEnumerable<PropertyInfo> props)
    {
      return props.Where(x => !x.IsSpecialName)
                  .Select(x => x.PropertyType)
                  .ToHashSet();
    }

    private HashSet<Type> GetMethodTypes(IEnumerable<MethodInfo> methodInfos)
    {

      var ret = new HashSet<Type>();

      var methods = methodInfos
        .Where(x => !x.IsSpecialName);

      foreach (var m in methods)
      {
        // Add the return type
        ret.Add(m.ReturnType);

        // Add the parameter types, skipping out parameters
        // TODO: work out why out parameters mess things up
        ret.UnionWith(m.GetParameters().Where(x => !x.IsOut).Select(x => x.ParameterType));
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

      var publicFacingTypes = new HashSet<Type>();

      // Methods
      var methods = Type.GetMethods().ToHashSet();

      // If the type is an interface, include the methods from extended interfaces.
      if (Type.IsInterface)
      {
        methods.UnionWith(Type.GetExtendedInterfaceMethods());
      }

      publicFacingTypes.UnionWith(GetMethodTypes(methods));

      // Properties
      // Not necessary in production because the properties will have been converted to methods
      // Do include properties for tests

      if (includeProperties)
      {
        publicFacingTypes.UnionWith(GetPropertyTypes(Type.GetProperties()));
      }

      publicFacingTypes.UnionWith(GetEventTypes(Type.GetEvents()));

      return FilterTypes(publicFacingTypes);
    }

    // TODO: Could be IEnumerable without T param eg. IControlGroup
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
