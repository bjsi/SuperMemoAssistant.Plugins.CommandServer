using NSubstitute;
using PythonCodeBuilder.Converter;
using PythonCodeBuilder.Converter.Transformers;
using PythonCodeBuilder.Converter.Transformers.Interfaces;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Plugins.CommandServer.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SuperMemoAssistant.Plugins.CommandServer.Tests.IntegrationTests
{
  public class ConvertGenerateTests
  {
    [Fact]
    public void ConvertServiceToPython() 
    {

      var svcType = typeof(IElement);
      var e = Substitute.For<IElement>();
      var compilerResults = Shared.CreateCompiler(e).WithAllAttributes().Compile();
      var type = compilerResults.CompiledAssembly.GetType($"{Shared.NameSpace}.{Shared.ClassName}");
      Assert.NotNull(type);

      var name = type.Name + "Svc";
      var publicFacingTypes = new TypeGenerator(typeof(IElement))
        .GetPublicFacingTypes(true)
        .ToDictionary(x => x, x => x.Name);

      var typeConverter = new PyTypeConverter(publicFacingTypes);

      var regMemberTypes = Shared.RegMemberToRegTypeMap.Keys.ToHashSet();

      var paramTransforms = new List<IMethodParameterTransformer> { new ThisIdTransformer(regMemberTypes) };
      var returnTransforms = new List<IMethodReturnTransformer> { new RegMemMethodReturnTransformer(regMemberTypes) };
      var consTransformer = new ConstructorTransformer(regMemberTypes);

      var converter = new PyConverter(name, type, svcType, paramTransforms, returnTransforms, typeConverter, consTransformer);
      converter.WithMethods();
      var output = converter.Convert();

      var pyMethods = output.Split('\n').Select(x => x.Trim()).Where(x => x.StartsWith("async def"));
      var csMethods = type.GetType().GetMethods().Where(x => !x.IsSpecialName && (x.DeclaringType == type));

      foreach (var method in csMethods)
      {
        Assert.True(pyMethods.Any(x => x.Contains(method.Name)));
      }
    }
  }
}
