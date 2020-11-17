using SuperMemoAssistant.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.CommandServer.Generator.Python
{
  public class PyTypeConverter
  {

    // Simple Types 
    // TODO: Incomplete
    public static Dictionary<Type, string> PrimitiveTypes = new Dictionary<Type, string>
    {
      { typeof(int), "int" },
      { typeof(IntPtr), "int"},
      { typeof(float), "float" },
      { typeof(double), "float" },
      { typeof(string), "str" },
      { typeof(bool), "bool" },
      { typeof(byte), "byte" },
      { typeof(void), "None" },
    };

    // SMA interop public-facing Types
    public HashSet<Type> PublicFacingTypes { get; }

    // TODO: Incomplete
    public Dictionary<Type, Func<Type, string>> GenericCollectionTypeConverters => new Dictionary<Type, Func<Type, string>>
    {
      { typeof(List<>), SequenceConverter },
      { typeof(IList<>), SequenceConverter },
      { typeof(IEnumerable<>), SequenceConverter },
      { typeof(Dictionary<,>), DictionaryConverter },
      { typeof(Array), ArrayConverter },
    };

    public PyTypeConverter(HashSet<Type> publicTypes)
    {
      publicTypes.ThrowIfNull("Failed to create python type converter because public types set was null");
      PublicFacingTypes = publicTypes;
    }

    public string Convert(Type type)
    {
      // Check if primitive
      if (PrimitiveTypes.ContainsKey(type))
      {
        return PrimitiveTypes[type];
      }

      // Check if public facing type
      else if (PublicFacingTypes.Contains(type))
      {
        return ConvertPublicFacingType(type);
      }

      else if (type.IsArray)
      {
        return ArrayConverter(type);
      }

      // Check if generic collection type
      else if (type.IsGenericType)
      {
        var genType = type.GetGenericTypeDefinition();
        if (GenericCollectionTypeConverters.ContainsKey(genType))
        {
          var converter = GenericCollectionTypeConverters[genType];
          return converter(type);
        }
        return null;
      }
      
      else
      {
        // unable to provide a type hint
        return null;
      }
    }

    private string ArrayConverter(Type type)
    {
      var arTyep = Convert(type.GetElementType());
      return $"List[{arTyep}]";
    }

    private string ConvertPublicFacingType(Type type)
    {
      return type.Name;
    }

    private string DictionaryConverter(Type type)
    {
      var key = type.GetGenericArguments()[0];
      var val = type.GetGenericArguments()[1];
      var pyKey = Convert(key);
      var pyVal = Convert(val);
      return $"Dict[{pyKey}, {pyVal}]";
    }

    public string SequenceConverter(Type type)
    {
      var arg = type.GetGenericArguments()[0];
      var pyArg = Convert(arg);
      return $"List[{pyArg}]";
    }
  }
}
