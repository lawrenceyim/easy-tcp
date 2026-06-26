using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EasyTcp;

public class JsonListener {
    public event Action<string> JsonReceived;
    public event Action<string> ClientConnected;
    public event Action<string> ClientDisconnected;

    private CancellationTokenSource _cancelTokenSource = new();

    private ByteListener _byteListener;

    public JsonListener(string ipAddress, int port) {
        _byteListener = new ByteListener(ipAddress, port);
        _byteListener.ClientConnected += (endpoint) => { ClientConnected?.Invoke(endpoint); };
        _byteListener.ClientDisconnected += (endpoint) => { ClientDisconnected?.Invoke(endpoint); };
        _byteListener.BytesReceived += (bytes) => {
            string result = Encoding.UTF8.GetString(bytes);
            JsonReceived?.Invoke(result);
        };
    }

    public void Start() {
        _byteListener.Start();
    }

    public void Stop() {
        _byteListener.Stop();
    }
}