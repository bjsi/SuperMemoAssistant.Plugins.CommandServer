using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Plugins.CommandServer.Helpers;
using Xunit;
using NSubstitute;

namespace SuperMemoAssistant.Plugins.CommandServer.Tests
{
  public class ReflectionTests
  {

    private ISuperMemoRegistry SuperMemoRegistry { get; } = Substitute.For<ISuperMemoRegistry>();

    [Fact]
    public void ReflectsCorrectRegistryMap()
    {
      var output = RefEx.CreateRegistryMap(SuperMemoRegistry);
      Assert.NotNull(output);
    }
  }
}
