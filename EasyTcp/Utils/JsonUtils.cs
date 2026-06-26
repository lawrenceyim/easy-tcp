using System.Net.Sockets;

namespace EasyTcp;

internal static class JsonUtils {
    internal static async Task<ITcpResult> ReadPacketLength(NetworkStream stream) {
        try {
            byte[] buffer = new byte[4];
            await stream.ReadExactlyAsync(buffer, 0, 4);
            return new SuccessIntDto(BitConverter.ToInt32(buffer, 0));
        }
        catch {
            return new FailureDto();
        }
    }

    internal static async Task<ITcpResult> ReadStringPacket(NetworkStream stream, int lengthOfStringInBytes) {
        try {
            byte[] buffer = new byte[lengthOfStringInBytes];
            await stream.ReadExactlyAsync(buffer, 0, lengthOfStringInBytes);
            return new SuccessJsonDto(BitConverter.ToString(buffer));
        }
        catch {
            return new FailureDto();
        }
    }
}

internal interface ITcpResult;

internal readonly struct SuccessIntDto(int value) : ITcpResult {
    internal int Value { get; } = value;
}

internal readonly struct SuccessJsonDto(string json) : ITcpResult {
    internal string Json { get; } = json;
}

internal readonly struct FailureDto(string message = "") : ITcpResult {
    internal string Message { get; } = message;
}