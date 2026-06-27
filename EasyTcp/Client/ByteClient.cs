using System.Net.Sockets;
using EasyTcp;

public class ByteClient : EasyTcpClient {
    public event Action<string, byte[]>? BytesReceived;
    public event Action<string, string>? ExceptionThrown;
    public event Action<string>? Disconnected;

    private readonly byte[] _buffer = new byte[1024];
    private TcpClient? _client;
    private NetworkStream? _stream;
    private CancellationTokenSource _cancelTokenSource = new();
    private CancellationToken _token;
    private string _endPoint = string.Empty;

    public bool Connect(string host, int port) {
        if (_client != null) {
            return false;
        }

        _client = new TcpClient(host, port);
        _stream = _client.GetStream();
        _cancelTokenSource = new CancellationTokenSource();
        _token = _cancelTokenSource.Token;
        _endPoint = $"{_client.Client.RemoteEndPoint}";

        Task.Run(() => {
            try {
                while (!_token.IsCancellationRequested) {
                    _stream.ReadExactly(_buffer, 0, 4);
                    int length = BufferReader.ReadInt(_buffer, 0);
                    _stream.ReadExactly(_buffer, 4, length);
                    BytesReceived?.Invoke(_endPoint, _buffer[4..(length + 4)]);
                }
            }
            catch (EndOfStreamException) {
                // Console.WriteLine($"{_endPoint} stream ended");
            }
            catch (Exception ex) {
                ExceptionThrown?.Invoke(_endPoint, ex.Message);
            }
            finally {
                _stream?.Close();
                _client?.Close();
                Disconnected?.Invoke(_endPoint);
            }
        }, _token);

        return true;
    }

    public bool Write(byte[] bytes) {
        try {
            if (_stream is null) {
                return false;
            }

            byte[] lengthBytes = BitConverter.GetBytes(bytes.Length);
            _stream.WriteAsync(lengthBytes, 0, lengthBytes.Length, _token);
            _stream.WriteAsync(bytes, 0, bytes.Length, _token);
        }
        catch {
            return false;
        }

        return true;
    }

    public void Stop() {
        _cancelTokenSource.Cancel();
        _stream?.Close();
        _client?.Close();
        Disconnected?.Invoke(_endPoint);
    }
}