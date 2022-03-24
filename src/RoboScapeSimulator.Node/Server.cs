using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RoboScapeSimulator.Node;

/// <summary>
/// Connection to Node.js-based communications program
/// </summary>
public class Server : IDisposable
{
    List<Action<Socket>> connectionCallbacks = new();

    /// <summary>
    /// Add a callback run when a socket connects
    /// </summary>
    /// <param name="callback"></param>
    public void OnConnection(Action<Socket> callback)
    {
        connectionCallbacks.Add(callback);
    }

    /// <summary>
    /// Node.js process
    /// </summary>
    Process? client;

    /// <summary>
    /// Thread handling input
    /// </summary>
    Thread? processThread;

    AnonymousPipeServerStream? pipeWriter;
    AnonymousPipeServerStream? pipeReader;

    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    /// <summary>
    /// Sockets known to this server
    /// </summary>
    ConcurrentDictionary<string, Socket> sockets = new();

    private bool disposedValue;

    /// <summary>
    /// Start the server
    /// </summary>
    public void Start()
    {
        if (client != null && !client.HasExited)
        {
            Trace.WriteLine("Server already running");
            return;
        }

        if (processThread != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource = new();
        }

        sw = null;
        pipeWriter = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable, 0x10000);
        pipeReader = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable, 0x10000);
        IAsyncEnumerator<string> reader;


        // Start Node.js process
        client = new Process();
        client.StartInfo.FileName = "node";
        client.StartInfo.Arguments = "./src/node/index.js " + pipeWriter.GetClientHandleAsString() + " " + pipeReader.GetClientHandleAsString();
        client.StartInfo.UseShellExecute = false;
        client.EnableRaisingEvents = true;

        client.Exited += (sender, args) =>
        {
            // Restart node if died
            Start();
        };

        client.Start();

        pipeWriter.DisposeLocalCopyOfClientHandle();
        pipeReader.DisposeLocalCopyOfClientHandle();

        processThread = new Thread(async () =>
        {
            var cancelToken = cancellationTokenSource.Token;

            reader = readPipe(pipeReader, cancellationTokenSource.Token).GetAsyncEnumerator();

            string message;
            var readerTask = reader.MoveNextAsync().AsTask();

            while (!cancelToken.IsCancellationRequested)
            {
                // Handle input from process
                var readerTaskTimeout = Task.WhenAny(Task.Delay(100), readerTask);

                if ((await readerTaskTimeout) == readerTask)
                {
                    message = reader.Current;

                    if (message == null)
                    {
                        continue;
                    }

                    if (message[0].ToString() == ((byte)ReceiveMessageType.Message).ToString())
                    {
                        var messageDataStart = message.IndexOf(' ');
                        string socketID = message.Substring(1, 20);
                        string messageType = message[21..messageDataStart];
                        string messageData = message[(messageDataStart + 1)..];
                        Debug.WriteLine(string.Concat(string.Concat("Message from ", socketID, " Received: Type: "), string.Concat(messageType, " Data: ", messageData)));

                        if (sockets.ContainsKey(socketID) && sockets[socketID].callbacks.ContainsKey(messageType))
                        {
                            JToken jData = JToken.ReadFrom(new JsonTextReader(new StringReader(messageData)));
                            sockets[socketID].callbacks[messageType].ForEach(callback =>
                            {
                                if (jData.Type == JTokenType.Array)
                                {
                                    callback(((JArray)jData).ToArray());
                                }
                                else
                                {
                                    callback(new JToken[] { jData });
                                }
                            });
                        }
                    }

                    if (message[0].ToString() == ((byte)ReceiveMessageType.SocketConnected).ToString())
                    {
                        Trace.WriteLine(string.Concat("New Socket Connected: ", message.AsSpan(1)));
                        connectionCallbacks.ForEach(callback =>
                        {
                            var socket = new Socket(this, message[1..]);

                            if (!sockets.ContainsKey(socket.ID))
                            {
                                sockets.TryAdd(socket.ID, socket);
                            }
                            callback(socket);
                        });
                    }

                    readerTask = reader.MoveNextAsync().AsTask();
                }

            }

            Debug.WriteLine("processThread ended");
        });
        processThread.Start();
    }

    static async IAsyncEnumerable<string> readPipe(AnonymousPipeServerStream pipe, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        StreamReader sr = new StreamReader(pipe);

        while (!cancellationToken.IsCancellationRequested)
        {
            string? line = null;
            while ((line = await sr.ReadLineAsync()) != null)
            {
                yield return line;
            }
        }
    }

    internal enum SendMessageType
    {
        Message
    }

    internal struct Message
    {
        string socketID;
        string data;
    }

    internal enum ReceiveMessageType
    {
        Message, SocketConnected
    }

    StreamWriter? sw;

    internal void send(string data)
    {
        if (sw == null)
        {
            sw = new(pipeWriter);
            sw.AutoFlush = true;
        }

        lock (sw)
        {
            if (pipeWriter != null)
            {
                try
                {
                    sw.WriteLine(data);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                }
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                cancellationTokenSource.Cancel();
                client?.Close();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// A Socket.io socket
/// </summary>
public class Socket
{
    /// <summary>
    /// Create a new Socket
    /// </summary>
    /// <param name="server">Server this socket belongs to</param>
    /// <param name="ID">ID of this socket</param>
    internal Socket(Server server, string ID)
    {
        this.server = server;
        this.ID = ID;
    }

    /// <summary>
    /// The server this Socket belongs to
    /// </summary>
    internal Server server;

    /// <summary>
    /// ID of this Socket
    /// </summary>
    public string ID;

    /// <summary>
    /// Callbacks for message types
    /// </summary>
    internal readonly Dictionary<JToken, List<Action<JToken[]>>> callbacks = new();

    /// <summary>
    /// Add a callback for an event
    /// </summary>
    /// <param name="eventName">Name of event</param>
    /// <param name="callback">Callback to run when event occurs</param>
    public void On(JToken eventName, Action<JToken[]> callback)
    {
        if (callbacks.ContainsKey(eventName))
        {
            callbacks[eventName].Add(callback);
        }
        else
        {
            callbacks.Add(eventName, new List<Action<JToken[]>>() { callback });
        }
    }

    /// <summary>
    /// Add a callback for an event
    /// </summary>
    /// <param name="eventName">Name of event</param>
    /// <param name="callback">Callback to run when event occurs</param>
    public void On(JToken eventName, Action callback)
    {
        On(eventName, (JToken[] args) => callback());
    }

    private readonly List<Action> onDisconnect = new();

    /// <summary>
    /// Setup a callback to run when this Socket disconnects
    /// </summary>
    /// <param name="callback">Callback to run when socket disconnects</param>
    public void OnDisconnect(Action callback)
    {
        onDisconnect.Add(callback);
    }

    /// <summary>
    /// Remove a callback from an event
    /// </summary>
    /// <param name="eventName">Event to remove callback from</param>
    /// <param name="callback">Callback to remove</param>
    public void Off(JToken eventName, Action<JToken[]> callback)
    {
        if (callbacks.ContainsKey(eventName))
        {
            callbacks[eventName].Remove(callback);
        }
    }

    /// <summary>
    /// Send an event to the client of this Socket
    /// </summary>
    /// <param name="eventName">Name of event to emit</param>
    /// <param name="data">Data to send</param>
    public void Emit(string eventName, JToken data)
    {
        string buffer = "0";
        buffer += ID;
        buffer += eventName;
        buffer += " ";
        buffer += data.ToString(Formatting.None);
        server.send(buffer);
    }

    /// <summary>
    /// Send an event to the client of this Socket
    /// </summary>
    /// <param name="eventName">Name of event to emit</param>
    public void Emit(string eventName)
    {
        Emit(eventName, "");
    }
}