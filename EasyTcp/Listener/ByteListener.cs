using System.Net;
using System.Net.Sockets;

namespace EasyTcp;

public class ByteListener : EasyTcpListener {
    public event Action<string, byte[]>? BytesReceived;
    public event Action<string>? ClientConnected;
    public event Action<string>? ClientDisconnected;
    public event Action<string>? ExceptionThrown;

    private readonly TcpListener _listener;
    private CancellationTokenSource _cancelTokenSource = new();
    private Dictionary<string, NetworkStream> _streams = [];

    public ByteListener(string ipAddress, int port) {
        _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
    }

    public void Start() {
        _listener.Start();
        _cancelTokenSource = new CancellationTokenSource();

        while (!_cancelTokenSource.IsCancellationRequested) {
            TcpClient client = _listener.AcceptTcpClient();
            Task.Run(() => { _StartListening(client, _cancelTokenSource.Token); }, _cancelTokenSource.Token);
        }
    }

    public void Stop() {
        _cancelTokenSource.Cancel();
        _listener.Stop();
        _streams.Clear();
    }

    public bool Write(string endPoint, byte[] bytes) {
        try {
            if (!_streams.TryGetValue(endPoint, out NetworkStream? stream)) {
                return false;
            }

            stream.WriteAsync(bytes, 0, bytes.Length);
        }
        catch {
            return false;
        }

        return true;
    }

    private Task _StartListening(TcpClient client, CancellationToken token) {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        string endPoint = $"{client.Client.RemoteEndPoint}";
        ClientConnected?.Invoke(endPoint);
        _streams.Add(endPoint, stream);

        try {
            while (!token.IsCancellationRequested) {
                stream.ReadExactly(buffer, 0, 4);
                int length = BufferReader.ReadInt(buffer, 0);
                stream.ReadExactly(buffer, 4, length);
                BytesReceived?.Invoke(endPoint, buffer[4..(length + 4)]);
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