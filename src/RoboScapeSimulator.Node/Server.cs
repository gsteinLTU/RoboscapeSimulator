using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RoboScapeSimulator.Node;

public class Server : IDisposable
{
    List<Action<Socket>> connectionCallbacks = new();

    public void OnConnection(Action<Socket> callback)
    {
        connectionCallbacks.Add(callback);
    }

    Process? client;

    Thread? processThread;

    AnonymousPipeServerStream? pipeWriter;
    AnonymousPipeServerStream? pipeReader;

    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    Dictionary<string, Socket> sockets = new();
    private bool disposedValue;

    public void Start()
    {
        if (client != null && !client.HasExited)
        {
            Trace.WriteLine("Server already running");
            return;
        }

        pipeWriter = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable, 0x8000);
        pipeReader = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable, 0x8000);

        client = new Process();

        client.StartInfo.FileName = "node";
        client.StartInfo.Arguments = "./src/node/index.js " + pipeWriter.GetClientHandleAsString() + " " + pipeReader.GetClientHandleAsString();
        client.StartInfo.UseShellExecute = false;
        client.Start();

        pipeWriter.DisposeLocalCopyOfClientHandle();
        pipeReader.DisposeLocalCopyOfClientHandle();

        processThread = new Thread(async () =>
        {
            var cancelToken = cancellationTokenSource.Token;

            var reader = readPipe(pipeReader, cancellationTokenSource.Token).GetAsyncEnumerator();

            string message;
            while (!cancelToken.IsCancellationRequested)
            {
                // Handle input from process
                var readerTask = reader.MoveNextAsync().AsTask();

                if (await readerTask)
                {
                    message = reader.Current;

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
                                sockets.Add(socket.ID, socket);
                            }
                            callback(socket);
                        });
                    }
                }
            }
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

        if (pipeWriter != null)
        {
            sw.WriteLine(data);
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
    internal Socket(Server server, string ID)
    {
        this.server = server;
        this.ID = ID;
    }

    internal Server server;

    public string ID;

    internal readonly Dictionary<JToken, List<Action<JToken[]>>> callbacks = new();

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

    public void On(JToken eventName, Action callback)
    {
        On(eventName, (JToken[] args) => callback());
    }

    private readonly List<Action> onDisconnect = new();

    public void OnDisconnect(Action callback)
    {
        onDisconnect.Add(callback);
    }

    public void Off(JToken eventName, Action<JToken[]> callback)
    {
        if (callbacks.ContainsKey(eventName))
        {
            callbacks[eventName].Remove(callback);
        }
    }

    public void Emit(string eventName, JToken data)
    {
        string buffer = "0";
        buffer += ID;
        buffer += eventName;
        buffer += " ";
        buffer += data.ToString(Formatting.None);
        server.send(buffer);
    }

    public void Emit(string eventName)
    {
        Emit(eventName, "");
    }
}