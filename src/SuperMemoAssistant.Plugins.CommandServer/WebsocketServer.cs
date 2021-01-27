using Anotar.Serilog;
using StreamJsonRpc;
using SuperMemoAssistant.Plugins.CommandServer.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.CommandServer
{
  // https://github.com/MV10/WebSocketExample/blob/master/WebSocketExample/WebSocketServer.cs
  public class WebsocketServer
  {
    private static HttpListener Listener;
    private static bool ServerIsRunning = true;

    private static int SocketCounter = 0;

    public CancellationTokenSource SocketLoopTokenSource { get; private set; }
    public CancellationTokenSource ListenerLoopTokenSource { get; private set; }

    // The key is a socket id
    private static ConcurrentDictionary<int, ConnectedClient> Clients = new ConcurrentDictionary<int, ConnectedClient>();

    private static int ServiceCounter = 0;
    private ConcurrentDictionary<int, object> Services = new ConcurrentDictionary<int, object>();
    
    public WebsocketServer(IEnumerable<object> services)
    {
      foreach (var service in services)
      {
        int serviceId = Interlocked.Increment(ref ServiceCounter);
        Services.TryAdd(serviceId, service);
      }
    }

    public int RegisterService(object service)
    {
      int serviceId = Interlocked.Increment(ref ServiceCounter);
      return Services.TryAdd(serviceId, service)
        ? serviceId
        : -1;
    }

    public bool RevokeService(int id)
    {
      return Services.TryRemove(id, out _);
    }

    public void Start(string host, int port)
    {
      SocketLoopTokenSource = new CancellationTokenSource();
      ListenerLoopTokenSource = new CancellationTokenSource();
      Listener = new HttpListener();
      string prefix = $"http://{host}:{port}/";
      Listener.Prefixes.Add(prefix);
      Listener.Start();
      if (Listener.IsListening)
      {
        LogTo.Debug($"Server listening: {prefix}");
        // listen on a separate thread so that Listener.Stop can interrupt GetContextAsync
        Task.Run(() => ListenerProcessingLoopAsync().ConfigureAwait(false));
      }
      else
      {
        LogTo.Debug("Command server failed to start.");
      }
    }

    private async Task SocketProcessingLoopAsync(ConnectedClient client)
    {
      var socket = client.Socket;
      var loopToken = SocketLoopTokenSource.Token; // recv async
      try
      {
        using (var handler = new WebSocketMessageHandler(client.Socket))
        using (var Rpc = new JsonRpc(handler))
        {
          Rpc.TraceSource.Switch.Level = SourceLevels.All;
          Rpc.TraceSource.Listeners.Add(new DefaultTraceListener());
          Rpc.AddLocalRpcTargets(Services.Values);
          Rpc.StartListening();
          await Rpc.Completion.ConfigureAwait(false);
        }
      }
      catch (OperationCanceledException)
      {
        // normal upon task/token cancellation, disregard
        await client.Socket
          .CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None)
          .ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Socket {client.SocketId}:");
      }
      finally
      {
        Console.WriteLine($"Socket {client.SocketId}: Ended processing loop in state {socket.State}");

        // don't leave the socket in any potentially connected state
        if (client.Socket.State != WebSocketState.Closed)
          client.Socket.Abort();

        // by this point the socket is closed or aborted, the ConnectedClient object is useless
        if (Clients.TryRemove(client.SocketId, out _))
          socket.Dispose();
      }
    }

    public async Task StopAsync()
    {
      if (Listener?.IsListening ?? false && ServerIsRunning)
      {
        LogTo.Debug("Command server is stopping");
        ServerIsRunning = false;            // prevent new connections during shutdown
        await CloseAllSocketsAsync();            // also cancels processing loop tokens (abort ReceiveAsync)
        ListenerLoopTokenSource.Cancel();   // safe to stop now that sockets are closed
        Listener.Stop();
        Listener.Close();
      }
    }

    private async Task CloseAllSocketsAsync()
    {
      // We can't dispose the sockets until the processing loops are terminated,
      // but terminating the loops will abort the sockets, preventing graceful closing.
      var disposeQueue = new List<WebSocket>(Clients.Count);

      while (Clients.Count > 0)
      {
        var client = Clients.ElementAt(0).Value;
        Console.WriteLine($"Closing Socket {client.SocketId}");

        Console.WriteLine("... ending broadcast loop");

        if (client.Socket.State != WebSocketState.Open)
        {
          Console.WriteLine($"... socket not open, state = {client.Socket.State}");
        }
        else
        {
          var timeout = new CancellationTokenSource(2500);
          try
          {
            Console.WriteLine("... starting close handshake");
            await client.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeout.Token);
          }
          catch (OperationCanceledException ex)
          {
            // normal upon task/token cancellation, disregard
          }
        }

        if (Clients.TryRemove(client.SocketId, out _))
        {
          // only safe to Dispose once, so only add it if this loop can't process it again
          disposeQueue.Add(client.Socket);
        }

        Console.WriteLine("... done");
      }

      // now that they're all closed, terminate the blocking ReceiveAsync calls in the SocketProcessingLoop threads
      SocketLoopTokenSource.Cancel();

      // dispose all resources
      foreach (var socket in disposeQueue)
        socket.Dispose();
    }

    private async Task ListenerProcessingLoopAsync()
    {
      var cancellationToken = ListenerLoopTokenSource.Token;
      try
      {
        while (!cancellationToken.IsCancellationRequested)
        {
          HttpListenerContext context = await Listener.GetContextAsync();
          if (ServerIsRunning)
          {
            if (context.Request.IsWebSocketRequest)
            {
              // HTTP is only the initial connection; upgrade to a client-specific websocket
              HttpListenerWebSocketContext wsContext = null;
              try
              {
                wsContext = await context.AcceptWebSocketAsync(subProtocol: null);
                int socketId = Interlocked.Increment(ref SocketCounter);
                var client = new ConnectedClient(socketId, wsContext.WebSocket);
                Clients.TryAdd(socketId, client);
                LogTo.Debug($"Command Server Socket {socketId}: New connection.");
                _ = Task.Run(() => SocketProcessingLoopAsync(client).ConfigureAwait(false));
              }
              catch (Exception)
              {
                // server error if upgrade from HTTP to WebSocket fails
                context.Response.StatusCode = 500;
                context.Response.StatusDescription = "WebSocket upgrade failed";
                context.Response.Close();
                return;
              }
            }
          }
          else
          {
            // HTTP 409 Conflict (with server's current state)
            context.Response.StatusCode = 409;
            context.Response.StatusDescription = "Server is shutting down";
            context.Response.Close();
            return;
          }
        }
      }
      catch (HttpListenerException ex) when (ServerIsRunning)
      {
        // Program.ReportException(ex);
      }
    }
  }
}
