using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RoboScapeSimulator.Node;

public class Server
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

    public void Start()
    {
        pipeWriter = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
        pipeReader = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);

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

                if (await Task.WhenAny(readerTask, Task.Delay(1000)) == readerTask)
                {
                    if (!readerTask.Result)
                    {
                        continue;
                    }

                    message = reader.Current;

                    Trace.WriteLine(message);

                    if (message[0].ToString() == ((byte)ReceiveMessageType.Message).ToString())
                    {
                        var messageDataStart = message.IndexOf(' ');
                        string socketID = message.Substring(1, 20);
                        string messageType = message[21..messageDataStart];
                        string messageData = message[(messageDataStart + 1)..];
                        Trace.WriteLine(string.Concat(string.Concat("Message for ", socketID, " Received: Type: "), string.Concat(messageType, " Data: ", messageData)));

                        if (sockets.ContainsKey(socketID))
                        {
                            JToken jData = JToken.ReadFrom(new JsonTextReader(new StringReader(messageData)));
                            sockets[socketID].callbacks[messageType]?.ForEach(callback =>
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
                            var socket = new Socket() { ID = message[1..] };

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
            var message = await sr.ReadLineAsync();

            if (message != null)
            {
                yield return message;
            }
        }
    }

    ~Server()
    {
        cancellationTokenSource.Cancel();
        client?.Close();
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

    internal void send()
    {

    }
}

/// <summary>
/// A Socket.io socket
/// </summary>
public class Socket
{
    private Server? server;

    public string? ID;

    internal Dictionary<JToken, List<Action<JToken[]>>> callbacks = new();

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

    private List<Action> onDisconnect = new();

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

    public void Emit(JToken eventName, JToken data)
    {

    }
}