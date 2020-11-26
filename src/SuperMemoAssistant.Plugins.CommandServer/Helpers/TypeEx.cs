using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.CommandServer.Helpers
{
  public static class TypeEx
  {
    public static bool IsRegistryMemberType(this Type type)
    {
      return Const.RegistryMemberTypes.Contains(type);
    }

    public static bool IsRegistryType(this Type type)
    {
      throw new NotImplementedException();
    }
  }
}
