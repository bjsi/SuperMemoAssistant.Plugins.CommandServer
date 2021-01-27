using Anotar.Serilog;
using StreamJsonRpc;
using System.Collections.Generic;

namespace SuperMemoAssistant.Plugins.CommandServer.Helpers
{
  public static class JsonRpcEx
  {
    public static void AddLocalRpcTargets(this JsonRpc rpc, IEnumerable<object> objs)
    {
      foreach (var obj in objs)
      {
        var name = obj.GetType().Name;
        rpc.AddLocalRpcTarget(obj,
                new JsonRpcTargetOptions
                {
                  NotifyClientOfEvents = true,
                  AllowNonPublicInvocation = false,
                  EventNameTransform = CommonMethodNameTransforms.Prepend(name),
                  MethodNameTransform = CommonMethodNameTransforms.Prepend(name)
                });

        LogTo.Debug($"Added {name} service");
      }
    }
  }
}
