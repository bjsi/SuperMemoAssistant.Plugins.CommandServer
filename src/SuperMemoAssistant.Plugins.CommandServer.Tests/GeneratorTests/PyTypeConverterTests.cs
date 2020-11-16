using SuperMemoAssistant.Plugins.CommandServer.Generator.Python;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SuperMemoAssistant.Plugins.CommandServer.Tests.GeneratorTests
{
  public class PyTypeConverterTests
  {
    private PyTypeConverter Converter = new PyTypeConverter(new HashSet<Type>());

    [Theory]
    [InlineData(typeof(List<int>), "List[int]")]
    public void ConvertListType(Type csType, string expectedPyType)
    {
      var actual = Converter.Convert(csType);
      Assert.Equal(expectedPyType, actual);
    }

    [Theory]
    [InlineData(typeof(Dictionary<int, string>), "Dict[int, str]")]
    public void ConvertDictionaryType(Type csType, string expectedPyType)
    {
      var actual = Converter.Convert(csType);
      Assert.Equal(expectedPyType, actual);
    }
  }
}
