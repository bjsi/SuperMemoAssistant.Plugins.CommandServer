using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.CommandServer.Generator.Python
{
  public class PythonParam
  {

    public string Name { get; }
    public Type CSType { get; }
    public string PyType { get; }

    public PythonParam(ParameterInfo info)
    {
      this.Name = info.Name;
      this.CSType = info.ParameterType;
      this.PyType = ConvertToPythonType(CSType);
    }

    private string ConvertToPythonType(Type csType)
    {
      return "";
    }
  }
}
