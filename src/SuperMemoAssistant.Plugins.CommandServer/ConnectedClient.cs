using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.CommandServer
{
  public class ConnectedClient
  {
    public ConnectedClient(int socketId, WebSocket socket)
    {
      SocketId = socketId;
      Socket = socket;
    }

    public int SocketId { get; private set; }

    public WebSocket Socket { get; private set; }
  }
}
