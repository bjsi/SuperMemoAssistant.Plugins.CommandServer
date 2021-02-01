# Command Server Plugin for SuperMemoAssistant
Uses dynamic compilation to make the SMA interop services available over a JSON-RPC websocket. 

The objectives behind creating the Command Server were:
- To make it easier to send data between other applications and SuperMemoAssistant.

The Command Server is very similar in its objectives to the AnkiConnect plugin for Anki, which provides an HTTP API for developers, making it easy to interact with Anki from external programs.

However, there are a number of features I added to the Command Server to improve the experience for developers whilst also (hopefully) making the project easier to maintain.

For developers:

1. The Command Server uses a websocket with JSON-RPC to enable bidirectional communication between clients and the Command Server.

- This allows multiple concurrent clients to call SMA methods, as well as listen for events.

2. The Command Server allows other SMA plugins to register service objects which can be compiled into JSON-RPC services at runtime, in a similar manner to the register and revoke  service functions already provided for core SMA plugins.

3. The Command Server automatically generates "registry member getter services" which allows the Command Server API for registry members to closely mirror the API available inside SMA.

For maintainers:

1. The Command Server uses reflection and dynamic compilation with System.CodeDom to transform the existing SMA services into JSON-RPC websocket services.

- The Command Server service compiler applies a generic set of transformations to the methods and properties of the SMA service objects, so recompilation should be the only necessary step to update the plugin after an SMA interop update.
