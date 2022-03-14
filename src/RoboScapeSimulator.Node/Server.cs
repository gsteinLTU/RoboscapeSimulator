using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.CompilerServices;

namespace RoboScapeSimulator.Node;

public class Server
{
    public void OnConnection(Action<Socket> callback){

    }

    Process? client;

    Thread? processThread;

    AnonymousPipeServerStream? pipeWriter;
    AnonymousPipeServerStream? pipeReader;

    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    public void Start(){
        pipeWriter = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
        pipeReader = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);

        client = new Process();

        client.StartInfo.FileName = "node";
        client.StartInfo.Arguments = "./node/index.js " + pipeWriter.GetClientHandleAsString() + " " + pipeReader.GetClientHandleAsString();

        client.StartInfo.UseShellExecute = false;
        client.Start();

        pipeWriter.DisposeLocalCopyOfClientHandle();
        pipeReader.DisposeLocalCopyOfClientHandle();

        processThread = new Thread(async () => {
            var token = cancellationTokenSource.Token;

            var reader = readPipe(pipeReader, cancellationTokenSource.Token).GetAsyncEnumerator();

            string message;
            while (!token.IsCancellationRequested)
            {
                // Handle input from process
                var readerTask = reader.MoveNextAsync().AsTask();
                if(await Task.WhenAny(readerTask, Task.Delay(1000)) == readerTask){
                    message = reader.Current;
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

    ~Server(){
        cancellationTokenSource.Cancel();
        client?.Close();
    }
}

public class Socket
{
    public void On(JToken eventName, Action<JToken[]> callback){

    }

    public void Off(JToken eventName, Action<JToken[]> callback){

    }

    public void Emit(JToken eventName, JToken data){

    }
}