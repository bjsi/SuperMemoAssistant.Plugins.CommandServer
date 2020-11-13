using SuperMemoAssistant.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperMemoAssistant.Plugins.CommandServer.Services.DI
{
  public class SvcContainer
  {
    private List<Service> _svcs { get; } = new List<Service>();

    /// <summary>
    /// Add an existing service to the list of services.
    /// </summary>
    /// <param name="svc">Existing Service</param>
    public void AddExistingSvc(Service svc)
    {
      svc.ThrowIfNull("Failed to add existing service because it was null");
      _svcs.Add(svc);
    }

    /// <summary>
    /// Add a Singleton service.
    /// One instance of the service is created - the same instance is returned each time it is resolved.
    /// </summary>
    /// <typeparam name="T">Service Type</typeparam>
    public void AddSingleton<T>()
    {
      AddSingleton(typeof(T));
    }

    public void AddSingleton(Type type)
    {
      _svcs.Add(new Service(type, SvcLifetime.Singleton));
    }

    /// <summary>
    /// Add a Transient service.
    /// A new instance of the service will be created each time it is resolved.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void AddTransient<T>()
    {
      _svcs.Add(new Service(typeof(T), SvcLifetime.Transient));
    }

    /// <summary>
    /// Get a service by its Type.
    /// TODO: Does this throw?
    /// </summary>
    /// <typeparam name="T">Service Type</typeparam>
    public Service GetSvc<T>()
    {
      var type = typeof(T);
      return GetSvc(type);
    }

    public IEnumerable<Service> GetAllSvcs()
    {
      return _svcs;
    }

    /// <summary>
    /// Get a service by its Type.
    /// Throws if no service is found.
    /// </summary>
    /// <param name="type">Service Type</param>
    /// <returns>Svc object or throws an exception</returns>
    public Service GetSvc(Type type)
    {
      var typeName = type.AssemblyQualifiedName;
      return _svcs.First(x => x.Type.AssemblyQualifiedName == typeName);
    }
  }

}
