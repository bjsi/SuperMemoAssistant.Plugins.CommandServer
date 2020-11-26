using NSubstitute;
using PythonCodeBuilder.Converter;
using PythonCodeBuilder.Converter.Transformers;
using PythonCodeBuilder.Converter.Transformers.Interfaces;
using SuperMemoAssistant.Plugins.CommandServer.Generator;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SuperMemoAssistant.Plugins.CommandServer.Tests.IntegrationTests.Generic
{
  public class GenericConvertGenerateTests<T>
    where T : class
  {

    private Type SvcType { get; } = typeof(T);
    private T MockObj { get; } = Substitute.For<T>();
    private CompilerResults Results { get; } 
    private Type Type { get; }
    private Dictionary<Type, string> PublicFacingTypes { get; }
    private PyTypeConverter TypeConverter { get; }
    private HashSet<Type> RegMemberTypes { get; }
    private List<IMethodParameterTransformer> MethodParamTransforms { get; }
    private List<IMethodReturnTransformer> MethodReturnTransforms { get; }
    private ConstructorTransformer ConsTransformer { get; }
    private PyConverter CodeConverter { get; }


    public GenericConvertGenerateTests()
    {
      Results = Shared.CreateCompiler(MockObj).WithAllAttributes().Compile();
      Type = Results.CompiledAssembly.GetType($"{Shared.NameSpace}.{Shared.ClassName}");


      PublicFacingTypes = new TypeGenerator(typeof(T))
        .GetPublicFacingTypes(true)
        .ToDictionary(x => x, x => x.Name);

      TypeConverter = new PyTypeConverter(PublicFacingTypes);

      RegMemberTypes = Shared.RegMemberToRegTypeMap.Keys.ToHashSet();

      MethodParamTransforms = new List<IMethodParameterTransformer> { new ThisIdTransformer(RegMemberTypes) };

      MethodReturnTransforms = new List<IMethodReturnTransformer> { new RegMemMethodReturnTransformer(RegMemberTypes) };

      ConsTransformer = new ConstructorTransformer(RegMemberTypes);

      CodeConverter = new PyConverter(Type.Name, Type, SvcType, MethodParamTransforms, MethodReturnTransforms, TypeConverter, ConsTransformer);

    }


    [Fact]
    public void AddsAllProperties()
    {
      CodeConverter.WithProperties();
      var output = CodeConverter.Convert();
      var lines = output.Split('\n').Select(x => x.Trim()).ToList();
      
      var csProps = Type.GetType().GetProperties().Where(x => !x.IsSpecialName && (x.DeclaringType == Type));

      foreach (var prop in csProps)
      {
        Assert.True(lines.Any(x => x.StartsWith(prop.Name)));
      }
    }

    [Fact]
    public void AddsConstructor()
    {

      CodeConverter.WithConstructor();
      var output = CodeConverter.Convert();
      var lines = output.Split('\n').Select(x => x.Trim()).ToList();

      int consLine = -1;
      for (int i = 0; i < lines.Count; i++)
      {
        if (lines[i].StartsWith("def __init__("))
          consLine = i;
      }

      Assert.True(consLine > -1);
      Assert.Equal("def __init__(self, json_rpc_dict: Dict[Any, Any]):", lines[consLine]);
      Assert.Equal("self._id = json_rpc_dict[\"Id\"]", lines[consLine + 1]);

    }

    [Fact]
    public void AddsAllMethods()
    {
      CodeConverter.WithMethods();
      var output = CodeConverter.Convert();

      var pyMethods = output.Split('\n').Select(x => x.Trim()).Where(x => x.StartsWith("async def"));
      var csMethods = Type.GetType().GetMethods().Where(x => !x.IsSpecialName && (x.DeclaringType == Type));

      foreach (var method in csMethods)
        Assert.True(pyMethods.Any(x => x.Contains(method.Name)));
    }
  }
}
