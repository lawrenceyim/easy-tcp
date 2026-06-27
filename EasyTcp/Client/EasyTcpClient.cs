public interface EasyTcpClient {
    public bool Connect(string host, int port);
    public bool Write(byte[] bytes);
    public void Stop();
}