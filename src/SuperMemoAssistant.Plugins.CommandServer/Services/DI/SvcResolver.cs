using SuperMemoAssistant.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperMemoAssistant.Plugins.CommandServer.Services.DI
{
  public class SvcResolver
  {
    private readonly SvcContainer _container;

    public SvcResolver(SvcContainer container)
    {
      container.ThrowIfNull("Failed to create service resolver because the service container was null");
      this._container = container;
    }

    /// <summary>
    /// Get all services
    /// </summary>
    public IEnumerable<object> GetAllSvcs()
    {
      return _container.GetAllSvcs().Select(x => GetSvc(x.Type));
    }

    /// <summary>
    /// Get a service by its Type.
    /// </summary>
    /// <typeparam name="T">Service Type</typeparam>
    /// <returns></returns>
    public T GetSvc<T>()
    {
      return (T)GetSvc(typeof(T));
    }

    public object CreateImplementation(Service svc, Func<Type, object> factory)
    {
      if (svc.Implemented)
      {
        return svc.Implementation;
      }

      var imp = factory(svc.Type);

      if (svc.Lifetime == SvcLifetime.Singleton)
      {
        svc.AddImplementation(imp);
      }

      return imp;
    }

    public object GetSvc(Type t)
    {
      var svc = _container.GetSvc(t);
      var svcType = svc.Type;
      var constructor = svcType.GetConstructors().Single();
      var parameters = constructor.GetParameters().ToArray();

      if (parameters.Any())
      {
        var paramImplementations = new object[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
          var p = parameters[i];
          var pType = p.ParameterType;
          paramImplementations[i] = GetSvc(pType);
        }
        return CreateImplementation(svc, t => Activator.CreateInstance(t, paramImplementations));
      }

      return CreateImplementation(svc, t => Activator.CreateInstance(t));
    }
  }
}
