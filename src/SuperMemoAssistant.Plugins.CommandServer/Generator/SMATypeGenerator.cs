using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Plugins.CommandServer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.CommandServer.Generator
{
  public class SMATypeGenerator 
  {

    private Dictionary<Type, Type> RegistryTypes { get; }

    public SMATypeGenerator(ISuperMemoRegistry reg)
    {
      RegistryTypes = RefEx.CreateRegistryMap(reg);
    }

    public HashSet<Type> GetPublicFacingTypes(bool includeProperties = false)
    {

      var ret = new HashSet<Type>();
      foreach (var type in RegistryTypes.Keys.ToList().Concat(RegistryTypes.Values))
      {
        var gen = new TypeGenerator(type);
        ret.UnionWith(gen.GetPublicFacingTypes(includeProperties));
      }

      return ret;

    }
  }
}
