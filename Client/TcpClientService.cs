using SharedLibrary;
using System.Net.Sockets;

namespace Client
{
    public sealed class TcpClientService : IAsyncDisposable
    {
        private readonly TcpClient _client = new();
        private NetworkStream? _stream;
        private readonly SemaphoreSlim _sendLock = new(1, 1);

        public bool IsConnected => _client.Connected;

        public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
        {
            await _client.ConnectAsync(host, port, cancellationToken);
            _stream = _client.GetStream();
        }

        public async Task<ServerResponse> SendAsync(ClientRequest request, CancellationToken cancellationToken = default)
        {
            NetworkStream stream = _stream ?? throw new InvalidOperationException("Client is not connected.");
            await _sendLock.WaitAsync(cancellationToken);
            try
            {
                await MessageFraming.SendAsync(stream, request, cancellationToken);
                return await MessageFraming.ReceiveAsync<ServerResponse>(stream, cancellationToken)
                    ?? throw new IOException("Server closed the connection.");
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            _stream?.Dispose();
            _client.Dispose();
            _sendLock.Dispose();
            await ValueTask.CompletedTask;
        }
    }
}
