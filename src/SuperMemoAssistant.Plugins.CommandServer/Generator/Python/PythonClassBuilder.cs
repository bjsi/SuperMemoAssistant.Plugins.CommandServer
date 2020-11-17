using SuperMemoAssistant.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperMemoAssistant.Plugins.CommandServer.Generator.Python
{
  public class PythonClassBuilder : PythonSourceGenerator
  {

    public const string BaseClassName = "SMA";
    public string ClassName { get; }

    public List<PythonClassMethod> Methods { get; } = new List<PythonClassMethod>();
    public List<PythonClassField> Properties { get; } = new List<PythonClassField>();

    private PyTypeConverter Converter { get; }
    private Type CSType { get; }
    
    public PythonClassBuilder(Type type, HashSet<Type> publicTypes, string name = null)
    {
      type.ThrowIfArgumentNull("Failed to create class builder because type was null");
      publicTypes.ThrowIfArgumentNull("Failed to create class builder because public types were null");
      Converter = new PyTypeConverter(publicTypes);
      CSType = type;
      ClassName = name ?? CSType.Name;
    }

    public PythonClassBuilder WithMethods()
    {
      foreach (var m in CSType.GetMethods().Where(x => !x.IsSpecialName))
      {
        var parameters = m.GetParameters();
        var pyParams = parameters.Select(x => new PythonParam(x));
        var name = m.Name;
        var csRetType = m.ReturnType;
        var pyRetType = Converter.Convert(csRetType);
        Methods.Add(new PythonClassMethod(name, pyParams, pyRetType, Array.Empty<string>(), true));
      }

      return this;
    }

    // Need to include fields as well?
    public PythonClassBuilder WithProperties()
    {
      foreach (var p in CSType.GetProperties())
      {
        var t = p.PropertyType;
        var name = p.Name;
        var pyType = Converter.Convert(t);
        Properties.Add(new PythonClassField(name, pyType));
      }

      return this;
    }

    public string GenerateSourceCode() => Generate("PythonClass.Mustache");
  }
}
