using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using StreamJsonRpc;
using SuperMemoAssistant.Plugins.CommandServer.Helpers;
using SuperMemoAssistant.Plugins.CommandServer.Services;
using SuperMemoAssistant.Plugins.CommandServer.Services.DI;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.Sentry;

#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   11/4/2020 9:50:13 PM
// Modified By:  james

#endregion




namespace SuperMemoAssistant.Plugins.CommandServer
{

  // ReSharper disable once UnusedMember.Global
  // ReSharper disable once ClassNeverInstantiated.Global
  [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
  public class CommandServerPlugin : SentrySMAPluginBase<CommandServerPlugin>
  {
    #region Constructors

    /// <inheritdoc />
    public CommandServerPlugin() : base("Enter your Sentry.io api key (strongly recommended)") { }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public override string Name => "CommandServer";

    /// <inheritdoc />
    public override bool HasSettings => false;

    public ConcurrentDictionary<string, object> Services { get; } = new ConcurrentDictionary<string, object>();

    private const string Host = "localhost";

    private const int Port = 13000;

    private HttpListener Listener { get; } = new HttpListener();

    private JsonRpc Rpc { get; set; }

    private static Dictionary<Type, Type> RegMemberToRegTypeMap { get; } = RefEx.CreateRegistryMap(Svc.SM.Registry);
    private HashSet<Type> Registries { get; } = RegMemberToRegTypeMap.Values.ToHashSet();
    private static SvcContainer Container { get; } = new SvcContainer();
    private SvcResolver Resolver { get; } = new SvcResolver(Container);


    #endregion




    #region Methods Impl

    /// <inheritdoc />
    protected override void PluginInit()
    {

      // Add registries as preexisting services
      foreach (var reg in Registries)
        Container.AddExistingSvc(new Service(reg));

      // Create interop service types
      var regMembers = CreateRegistryMemberTypes();
      var regRepos = CreateRegistrySvcTypes();
      var uiSvc = CreateUISvcTypes();

      // Add interop service types as Singleton services
      foreach (var x in regMembers.Concat(regRepos).Concat(uiSvc))
        Container.AddSingleton(x);

      // Begin the websocket server
      Task.Run(async () => await StartListeningAsync().ConfigureAwait(false));

      // Uncomment to register an event handler which will be notified when the displayed element changes
      // Svc.SM.UI.ElementWdw.OnElementChanged += new ActionProxy<SMDisplayedElementChangedEventArgs>(OnElementChanged);
    }

    private List<Type> CreateUISvcTypes()
    {

      var ret = new List<Type>();
      foreach (var ui in RefEx.CreateUIList(Svc.SM.UI))
      {
        var fac = new SvcTypeFactory(ui, ui.GetType());
        ret.Add(fac.Compile());
      }
      return ret;

    }

    private List<Type> CreateRegistrySvcTypes()
    {
      List<Type> svcTypes = new List<Type>();

      foreach (var rejObj in Registries)
      {
        var fac = new SvcTypeFactory(rejObj, rejObj.GetType());
        svcTypes.Add(fac.Compile());
      }

      return svcTypes;
    }

    private List<Type> CreateRegistryMemberTypes()
    {
      List<Type> svcTypes = new List<Type>();
      foreach (var regMemType in RegMemberToRegTypeMap.Keys)
      {
        var fac = new SvcTypeFactory(null, regMemType.GetType());
        svcTypes.Add(fac.Compile());
      }
      return svcTypes;
    }

    private async Task StartListeningAsync()
    {
      Listener.Start();
      Listener.Prefixes.Add($"http://{Host}:{Port}/");

      while (true)
      {

        HttpListenerContext context = await Listener.GetContextAsync().ConfigureAwait(false);

        if (context.Request.IsWebSocketRequest)
        {
          HttpListenerWebSocketContext webSocketContext = await context
            .AcceptWebSocketAsync(null)
            .ConfigureAwait(false);

          WebSocket webSocket = webSocketContext.WebSocket;
          using (Rpc = new JsonRpc(new WebSocketMessageHandler(webSocket)))
          {
            try
            {
              Rpc.TraceSource.Switch.Level = SourceLevels.All;
              Rpc.TraceSource.Listeners.Add(new DefaultTraceListener());
              Rpc.AddLocalRpcTargets(Resolver.GetAllSvcs());
              Rpc.StartListening();
              Console.WriteLine("JSON-RPC protocol over web socket established.");

              await Rpc.Completion.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
              // Close the web socket gracefully -- before JsonRpc is disposed to avoid the socket going into an aborted state.
              await webSocket
                .CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None)
                .ConfigureAwait(false);
            }
          }
        }
      }
    }

      // Set HasSettings to true, and uncomment this method to add your custom logic for settings
      // /// <inheritdoc />
      // public override void ShowSettings()
      // {
      // }

      #endregion




      #region Methods

      // Uncomment to register an event handler for element changed events
      // [LogToErrorOnException]
      // public void OnElementChanged(SMDisplayedElementChangedEventArgs e)
      // {
      //   try
      //   {
      //     Insert your logic here
      //   }
      //   catch (RemotingException) { }
      // }

      #endregion
  }
}
