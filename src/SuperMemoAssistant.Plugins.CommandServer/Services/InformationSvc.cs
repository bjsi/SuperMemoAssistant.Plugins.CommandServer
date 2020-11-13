using SuperMemoAssistant.Plugins.CommandServer.Helpers;
using SuperMemoAssistant.Plugins.CommandServer.Models;
using SuperMemoAssistant.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SuperMemoAssistant.Plugins.CommandServer.Services
{
  public class InformationSvc
  {
    private ConcurrentDictionary<string, object> Services => Svc<CommandServerPlugin>.Plugin.Services;
    public List<Method> GetMethods(string serviceName)
    {
      var ret = new List<Method>();

      foreach (var obj in Services.Values)
      {
        Type type = obj.GetType();
        if (type.Name != serviceName)
          continue;

        var methods = type.GetMethods()
            ?.Where(x => x.DeclaringType == type)
            ?.Where(x => !x.IsSpecialName);

        if (methods == null)
          continue;

        foreach (var m in methods)
        {
          var name = m.Name;
          var retType = m.ReturnType.ToGenericTypeString();
          var parameters = m.GetParameters();
          var paramInfo = GetParameterInfo(parameters);
          ret.Add(new Method(name, retType, paramInfo));
        }
      }

      return ret;
    }

    private List<Param> GetParameterInfo(ParameterInfo[] parameters)
    {
      var ret = new List<Param>();
      foreach (var info in parameters)
      {
        var name = info.Name;
        var type = info.ParameterType.ToGenericTypeString();
        ret.Add(new Param(name, type));
      }

      return ret;
    }

    public List<string> GetServices()
    {
      return Services.Values
          ?.Select(x => x.GetType().Name)
          ?.ToList();
    }
  }
}
