using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.CommandServer.Generator.Python
{
  public class PythonClassMethod
  {
    public bool IsAsync { get; }
    public string Name { get; }
    public IEnumerable<PythonParam> Parameters { get; }
    public string ParameterString => string.Join(",", Parameters.Select(x => $"{x.Name}: {x.PyType}"));
    public string[] Statements { get; } // If no statements, pass

    public PythonClassMethod(string name, IEnumerable<PythonParam> parameters, string[] statements, bool async = false)
    {
      this.Name = name;
      this.Parameters = parameters;
      this.IsAsync = async;
      this.Statements = statements;
    }
  }
}
