using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EasyTcp;

public class JsonListener : EasyTcpListener {
    public event Action<string, string>? JsonReceived;
    public event Action<string>? ClientConnected;
    public event Action<string>? ClientDisconnected;
    public event Action<string>? ExceptionThrown;

    private CancellationTokenSource _cancelTokenSource = new();

    private ByteListener _byteListener;

    public JsonListener(string ipAddress, int port) {
        _byteListener = new ByteListener(ipAddress, port);
        _byteListener.ClientConnected += (endpoint) => { ClientConnected?.Invoke(endpoint); };
        _byteListener.ClientDisconnected += (endpoint) => { ClientDisconnected?.Invoke(endpoint); };
        _byteListener.ExceptionThrown += (e) => { ExceptionThrown?.Invoke(e); };
        _byteListener.BytesReceived += (endPoint, bytes) => {
            string result = Encoding.UTF8.GetString(bytes);
            JsonReceived?.Invoke(endPoint, result);
        };
    }

    public void Start() {
        _byteListener.Start();
    }

    public void Stop() {
        _byteListener.Stop();
    }

    public bool Write(string endPoint, byte[] bytes) {
        return _byteListener.Write(endPoint, bytes);
    }

    public void Write(string endPoint, string json) {
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        _byteListener.Write(endPoint, bytes);
    }
}