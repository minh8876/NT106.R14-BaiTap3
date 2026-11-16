using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace SharedLibrary
{
    public static class MessageFraming
    {
        private const int MaxMessageBytes = 1024 * 1024;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public static async Task SendAsync<T>(NetworkStream stream, T message, CancellationToken cancellationToken = default)
        {
            byte[] payload = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);
            if (payload.Length > MaxMessageBytes)
            {
                throw new InvalidDataException("Message exceeds the maximum permitted size.");
            }

            byte[] header = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(header, payload.Length);
            await stream.WriteAsync(header, cancellationToken);
            await stream.WriteAsync(payload, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }

        public static async Task<T?> ReceiveAsync<T>(NetworkStream stream, CancellationToken cancellationToken = default)
        {
            byte[] header = new byte[sizeof(int)];
            if (!await ReadExactlyOrEndAsync(stream, header, cancellationToken))
            {
                return default;
            }

            int length = BinaryPrimitives.ReadInt32BigEndian(header);
            if (length <= 0 || length > MaxMessageBytes)
            {
                throw new InvalidDataException("Invalid message length.");
            }

            byte[] payload = new byte[length];
            await stream.ReadExactlyAsync(payload, cancellationToken);
            return JsonSerializer.Deserialize<T>(payload, JsonOptions)
                ?? throw new InvalidDataException("Message payload is empty or invalid.");
        }

        private static async Task<bool> ReadExactlyOrEndAsync(NetworkStream stream, byte[] buffer, CancellationToken cancellationToken)
        {
            int offset = 0;
            while (offset < buffer.Length)
            {
                int read = await stream.ReadAsync(buffer.AsMemory(offset), cancellationToken);
                if (read == 0)
                {
                    return false;
                }

                offset += read;
            }

            return true;
        }
    }
}
