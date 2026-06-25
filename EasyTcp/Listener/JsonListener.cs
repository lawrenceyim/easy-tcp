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
            Task.Run(() => { _HandleClient(client, _cancelTokenSource.Token); }, _cancelTokenSource.Token);
        }
    }

    public void Stop() {
        _cancelTokenSource.Cancel();
        _listener.Stop();
    }

    private void _HandleClient(TcpClient client, CancellationToken token) {
        NetworkStream stream = client.GetStream();
        try {
            while (!token.IsCancellationRequested) {
                ITcpResult result = JsonUtils.ReadPacketLength(stream);
                if (result is not SuccessIntDto successIntDto) {
                    continue;
                }

                int length = successIntDto.Value;
                result = JsonUtils.ReadStringPacket(stream, length);
                if (result is not SuccessJsonDto successJsonDto) {
                    continue;
                }

                string json = successJsonDto.Json;
                JsonReceived?.Invoke(json);
            }
        }
        catch {
            // ignored
        }
        finally {
            stream.Close();
        }
    }
}