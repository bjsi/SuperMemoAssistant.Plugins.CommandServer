using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.CommandServer.Generator.Python
{
  public class PythonClassField
  {

    private string Name { get; }
    private string PyType { get; }

    public PythonClassField(string name, string type)
    {
      Name = name;
      PyType = type;
    }
  }
}
