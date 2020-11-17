using SuperMemoAssistant.Interop.SuperMemo.Content;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Plugins.CommandServer.Generator;
using SuperMemoAssistant.Plugins.CommandServer.Generator.Python;
using System;
using System.Collections.Generic;
using Xunit;

namespace SuperMemoAssistant.Plugins.CommandServer.Tests.GeneratorTests
{
  public class PyTypeConverterTests
  {
    private PyTypeConverter Converter { get;  }

    public PyTypeConverterTests()
    {
      var pubTypes = new SMATypeGenerator(Shared.SuperMemoRegistry).GetPublicFacingTypes(true);
      Converter = new PyTypeConverter(pubTypes);
    }

    [Theory]
    [InlineData(typeof(List<int>), "List[int]")]
    [InlineData(typeof(List<List<int>>), "List[List[int]]")]
    public void ConvertListType(Type csType, string expectedPyType)
    {
      var actual = Converter.Convert(csType);
      Assert.Equal(expectedPyType, actual);
    }

    [Theory]
    [InlineData(typeof(int[]), "List[int]")]
    [InlineData(typeof(string[]), "List[str]")]
    [InlineData(typeof(ElementBuilder[]), "List[ElementBuilder]")] // TODO: Need to fix out param
    [InlineData(typeof(IElement[]), "List[IElement]")]
    public void ConvertArrayType(Type csType, string expectedPyType)
    {
      var actual = Converter.Convert(csType);
      Assert.Equal(expectedPyType, actual);
    }

    [Theory]
    [InlineData(typeof(Dictionary<int, string>), "Dict[int, str]")]
    [InlineData(typeof(Dictionary<int, Dictionary<int, int>>), "Dict[int, Dict[int, int]]")]
    [InlineData(typeof(Dictionary<int, IComponentGroup>), "Dict[int, IComponentGroup]")]
    public void ConvertDictionaryType(Type csType, string expectedPyType)
    {
      var actual = Converter.Convert(csType);
      Assert.Equal(expectedPyType, actual);
    }
  }
}
