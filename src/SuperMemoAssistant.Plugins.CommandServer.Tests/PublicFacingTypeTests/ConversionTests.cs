using PythonCodeBuilder.Converter;
using PythonCodeBuilder.Converter.Transformers.Interfaces;
using SuperMemoAssistant.Plugins.CommandServer.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SuperMemoAssistant.Plugins.CommandServer.Tests.PublicFacingTypeTests
{
  public class ConversionTests
  {
    [Fact]
    public void ConvertsPublicFacingEnumTypes()
    {
      var publicTypes = new SMATypeGenerator(Shared.SuperMemoRegistry).GetPublicFacingTypes(true).Where(x => x.IsEnum);
      foreach (var type in publicTypes)
      {
        var name = type.Name;
        var pyTypeConverter = new PyTypeConverter();
        var converter = new PyConverter(name, type, type, new List<IMethodParameterTransformer>(), new List<IMethodReturnTransformer>(), pyTypeConverter);
        converter.WithFields();
        var output = converter.Convert();
        var lines = output.Split('\n').Select(x => x.Trim());

        Type enumUnderlyingType = Enum.GetUnderlyingType(type);
        var names = Enum.GetNames(type);
        var values = Enum.GetValues(type);
        if (names.Length != values.Length)
            throw new Exception("Unexpected length mismatch");

        for (int i = 0; i < names.Length; i++)
        {
          var pyField = lines.Where(x => x.StartsWith(names[i])).FirstOrDefault();
          Assert.NotNull(pyField);

          // Retrieve the value of the ith enum item.
          object value = values.GetValue(i);

          // Convert the value to its underlying type (int, byte, long, ...)
          object underlyingValue = Convert.ChangeType(value, enumUnderlyingType);

          string expectedPyValue = underlyingValue.ToString();

          Assert.Equal(expectedPyValue, pyField.Split('=')[1].Trim());
        }
      }
    }
  }
}
