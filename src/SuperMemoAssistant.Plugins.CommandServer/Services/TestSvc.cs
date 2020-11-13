using System.Diagnostics;

namespace SuperMemoAssistant.Plugins.CommandServer.Services
{
  public class TestSvc
  {
    public int Add(int a, int b)
    {
      Debug.WriteLine("Calling add...");
      var x = a + b;
      return x;
    }
  }
}
