using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Plugins.CommandServer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.CommandServer.Services.DI
{
  public enum SvcLifetime
  {
    Singleton = 0,
    Transient = 1,
  }

  public class Service
  {
    public Type Type { get; set; }
    public SvcLifetime Lifetime { get; set; }
    public object Implementation { get; set; }
    public bool Implemented { get; set; }

    public void AddImplementation(object i)
    {
      Implementation = i;
      Implemented = true;
    }

    public Service(Type type, SvcLifetime lifetime)
    {
      this.Type = type;
      this.Lifetime = lifetime;
    }

    public Service(object implementation)
    {
      implementation.ThrowIfNull("Failed to create service because existing implementation was null");
      this.Type = implementation.GetType();
      this.Lifetime = SvcLifetime.Singleton;
      this.Implementation = implementation;
      this.Implemented = true;
    }
  }
}
