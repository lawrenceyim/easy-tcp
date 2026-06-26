using System.Text;

namespace EasyTcp;

public static class BufferReader {
    public static int ReadInt(byte[] buffer, int index) {
        return BitConverter.ToInt32(buffer, index);
    }

    public static string ReadString(byte[] buffer, int index, int length) {
        return Encoding.UTF8.GetString(buffer, index, length);
    }
}