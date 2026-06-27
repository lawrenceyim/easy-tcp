using System.Text;

namespace EasyTcp;

public class JsonClient : EasyTcpClient {
    public event Action<string, byte[]>? BytesReceived;
    public event Action<string, string>? ExceptionThrown;
    public event Action<string>? Disconnected;

    private readonly ByteClient _byteClient;

    public JsonClient() {
        _byteClient = new ByteClient();
        _byteClient.BytesReceived += (endPoint, bytes) => BytesReceived?.Invoke(endPoint, bytes);
        _byteClient.ExceptionThrown += (endPoint, exception) => ExceptionThrown?.Invoke(endPoint, exception);
        _byteClient.Disconnected += (endPoint) => Disconnected?.Invoke(endPoint);
    }

    public bool Connect(string host, int port) {
        return _byteClient.Connect(host, port);
    }

    public bool Write(byte[] bytes) {
        return _byteClient.Write(bytes);
    }

    public bool Write(string json) {
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        return _byteClient.Write(bytes);
    }


    public void Stop() {
        _byteClient.Stop();
    }
}