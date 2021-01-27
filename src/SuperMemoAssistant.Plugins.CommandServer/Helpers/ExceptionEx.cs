using System;

namespace SuperMemoAssistant.Plugins.CommandServer.Helpers
{
  public static class ExceptionEx
  {
    public static void ThrowIfNull(this object o, string msg)
    {
      if (o == null)
        throw new ArgumentNullException(msg);
    }
  }
}
