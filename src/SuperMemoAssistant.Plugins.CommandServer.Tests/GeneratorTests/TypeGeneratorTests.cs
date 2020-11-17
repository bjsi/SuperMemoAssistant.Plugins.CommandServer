using SuperMemoAssistant.Interop.SuperMemo.Content;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Elements;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.Learning;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.Interop.SuperMemo.UI.Element;
using SuperMemoAssistant.Plugins.CommandServer.Generator;
using SuperMemoAssistant.Plugins.CommandServer.Generator.Python;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xunit;

namespace SuperMemoAssistant.Plugins.CommandServer.Tests.GeneratorTests
{
  public class TypeGeneratorTests
  {
    [Fact]
    
    public void GetsIElementRegistryRequiredTypes()
    {
      var expected = new HashSet<Type>
      {
        typeof(IElement),
        typeof(ElemCreationResult), // TODO: Misses the out parameter
        typeof(ElemCreationFlags),
        typeof(SMElementEventArgs),
        typeof(SMElementChangedEventArgs),
        typeof(Regex)
      };

      var actual = new TypeGenerator(typeof(IElementRegistry)).GetPublicFacingTypes(true);

      Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetsIElementRequiredTypes()
    {
      var expected = new HashSet<Type>
      {
        typeof(ElementType),
        typeof(IComponentGroup),
        typeof(IElement),
        typeof(ITemplate),
        typeof(IConcept),
        typeof(SMElementChangedEventArgs)
      };

      var actual = new TypeGenerator(typeof(IElement)).GetPublicFacingTypes(true);

      Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetsIConceptRequiredTypes()
    {
      var expected = new HashSet<Type>
      {
        typeof(IConceptGroup),
        typeof(IElement)
      };

      var actual = new TypeGenerator(typeof(IConcept)).GetPublicFacingTypes(true);

      Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetsIElementWdwRequiredTypes()
    {
      var expected = new HashSet<Type>
      {
        typeof(IControlGroup),
        typeof(IElement),
        typeof(LearningMode),
        typeof(ElementDisplayState),
        typeof(SMDisplayedElementChangedEventArgs),
        typeof(ElementDisplayState),
      };

      var actual = new TypeGenerator(typeof(IElementWdw)).GetPublicFacingTypes(true);
      Assert.Equal(expected, actual);
    }

  }
}
