using SuperMemoAssistant.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.CommandServer.Generator.Python
{
  public class SvcGenerator
  {

    private readonly Type Type;

    public SvcGenerator(Type type)
    {
      type.ThrowIfNull("");
      this.Type = type;
    }
  }
}
