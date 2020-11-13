using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.CommandServer.Models
{
  [Serializable]
  public class Method
  {
    public Method(string name, string returnType, List<Param> parameters) // string comment) // Possible but more complex
    {
      Name = name;
      ReturnType = returnType;
      Parameters = parameters;
      // Comment = comment;
    }

    //[JsonProperty("comment")]
    //public string Comment { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("return_type")]
    public string ReturnType { get; set; }

    [JsonProperty("parameters")]
    public List<Param> Parameters { get; set; }

  }
}
