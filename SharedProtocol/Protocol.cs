using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharedProtocol
{
    public static class ProtocolCommands
    {
        public const string DANGKY = "DANGKY";
        public const string DANGNHAP = "DANGNHAP";
        public const string LAYTHONGTIN = "LAYTHONGTIN";
        public const string DANGXUAT = "DANGXUAT";
    }

    public static class ProtocolResponses
    {
        public const string SUCCESS = "SUCCESS";
        public const string ERR_USER_EXISTS = "ERR_USER_EXISTS";
        public const string ERR_INVALID_INPUT = "ERR_INVALID_INPUT";
        public const string ERR_AUTH_FAILED = "ERR_AUTH_FAILED";
        public const string ERR_SERVER_ERROR = "ERR_SERVER_ERROR";
        public const string ERR_UNAUTHORIZED = "ERR_UNAUTHORIZED";
        public const string ERR_NOT_FOUND = "ERR_NOT_FOUND";
    }

    public static class MessageFraming
    {
        public static async Task SendMessageAsync(NetworkStream stream, string message, CancellationToken cancellationToken = default)
        {
            byte[] payload = Encoding.UTF8.GetBytes(message);
            byte[] lengthPrefix = BitConverter.GetBytes(payload.Length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthPrefix);
            }
            await stream.WriteAsync(lengthPrefix, 0, 4, cancellationToken);
            await stream.WriteAsync(payload, 0, payload.Length, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }

        public static async Task<string> ReceiveMessageAsync(NetworkStream stream, CancellationToken cancellationToken = default)
        {
            byte[] lengthPrefix = new byte[4];
            int bytesRead = 0;
            while (bytesRead < 4)
            {
                int read = await stream.ReadAsync(lengthPrefix, bytesRead, 4 - bytesRead, cancellationToken);
                if (read == 0) throw new IOException("Connection closed by remote host.");
                bytesRead += read;
            }

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthPrefix);
            }
            int length = BitConverter.ToInt32(lengthPrefix, 0);

            byte[] payload = new byte[length];
            bytesRead = 0;
            while (bytesRead < length)
            {
                int read = await stream.ReadAsync(payload, bytesRead, length - bytesRead, cancellationToken);
                if (read == 0) throw new IOException("Connection closed by remote host.");
                bytesRead += read;
            }

            return Encoding.UTF8.GetString(payload);
        }
    }
}