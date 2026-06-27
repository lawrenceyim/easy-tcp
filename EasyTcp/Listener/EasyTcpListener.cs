public interface EasyTcpListener {
    public void Start();
    public void Stop();
    public bool Write(string endPoint, byte[] bytes);
}
