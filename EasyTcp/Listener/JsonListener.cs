using System.Net;
using System.Net.Sockets;

namespace EasyTcp;

public class JsonListener {
    public event Action<string> JsonReceived;

    private readonly TcpListener _listener;
    private CancellationTokenSource _cancelTokenSource = new();

    public JsonListener(string ipAddress, int port) {
        _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
    }

    public void Start() {
        _listener.Start();
        _cancelTokenSource = new CancellationTokenSource();

        while (!_cancelTokenSource.IsCancellationRequested) {
            TcpClient client = _listener.AcceptTcpClient();
            Console.WriteLine($"{client.Client.RemoteEndPoint} connected");
            Task.Run(() => { _ = _HandleClient(client, _cancelTokenSource.Token); }, _cancelTokenSource.Token);
        }
    }

    public void Stop() {
        _cancelTokenSource.Cancel();
        _listener.Stop();
    }

    private async Task _HandleClient(TcpClient client, CancellationToken token) {
        Console.WriteLine($"Handling client: {client.Client.RemoteEndPoint}");
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        try {
            Console.WriteLine($"{client.Client.RemoteEndPoint} try");
            while (!token.IsCancellationRequested) {
                int bytesRead = stream.Read(buffer);
                if (bytesRead == 0) {
                    Console.WriteLine($"{client.Client.RemoteEndPoint} bytes read == 0");
                    break;
                }

                int length = BufferReader.ReadInt(buffer, 0);
                string json = BufferReader.ReadString(buffer, 4, length);
                JsonReceived?.Invoke(json);
            }
        }
        catch (Exception e) {
            Console.WriteLine($"{client.Client.RemoteEndPoint} catch with exception: {e}");
            // ignored
        }
        finally {
            Console.WriteLine($"{client.Client.RemoteEndPoint} disconnected");
            stream.Close();
        }
    }
}