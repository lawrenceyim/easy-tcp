using System.Net;
using System.Net.Sockets;

namespace EasyTcp;

public class JsonListener {
    public event Action<string> JsonReceived;
    public event Action<string> ClientConnected;
    public event Action<string> ClientDisconnected;

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
            Task.Run(() => { _ = _HandleClient(client, _cancelTokenSource.Token); }, _cancelTokenSource.Token);
        }
    }

    public void Stop() {
        _cancelTokenSource.Cancel();
        _listener.Stop();
    }

    private Task _HandleClient(TcpClient client, CancellationToken token) {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        string endPoint = $"{client.Client.RemoteEndPoint}";
        ClientConnected?.Invoke(endPoint);

        try {
            // Console.WriteLine($"{client.Client.RemoteEndPoint} try");
            while (!token.IsCancellationRequested) {
                stream.ReadExactly(buffer, 0, 4);
                int length = BufferReader.ReadInt(buffer, 0);
                stream.ReadExactly(buffer, 4, length);
                string json = BufferReader.ReadString(buffer, 4, length);
                JsonReceived?.Invoke(json);
            }
        }
        catch (EndOfStreamException) {
            // Console.WriteLine($"{endPoint} stream ended");
        }
        catch (Exception e) {
            // Console.WriteLine($"{endPoint} catch with exception: {e}");
        }
        finally {
            ClientDisconnected.Invoke(endPoint);
            stream.Close();
        }

        return Task.CompletedTask;
    }
}