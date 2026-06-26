using System.Net;
using System.Net.Sockets;

namespace EasyTcp;

public class ByteListener {
    public event Action<byte[]>? BytesReceived;
    public event Action<string>? ClientConnected;
    public event Action<string>? ClientDisconnected;
    public event Action<string>? ExceptionThrown;

    private readonly TcpListener _listener;
    private CancellationTokenSource _cancelTokenSource = new();

    public ByteListener(string ipAddress, int port) {
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
            while (!token.IsCancellationRequested) {
                stream.ReadExactly(buffer, 0, 4);
                int length = BufferReader.ReadInt(buffer, 0);
                stream.ReadExactly(buffer, 4, length);
                BytesReceived?.Invoke(buffer[4..(length + 4)]);
            }
        }
        catch (EndOfStreamException) {
            // Console.WriteLine($"{endPoint} stream ended");
        }
        catch (Exception e) {
            ExceptionThrown?.Invoke(e.ToString());
        }
        finally {
            ClientDisconnected?.Invoke(endPoint);
            stream.Close();
        }

        return Task.CompletedTask;
    }
}