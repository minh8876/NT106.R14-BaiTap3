using Server.Handlers;
using SharedLibrary;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public sealed class TcpServer
    {
        private readonly int _port;
        private readonly TcpListener _listener;
        private readonly DatabaseService? _database;
        private readonly ConcurrentDictionary<string, TcpClient> _clients = new();
        private readonly ConcurrentDictionary<string, Task> _clientHandlers = new();
        private readonly ConcurrentDictionary<string, UserDto> _sessions = new();
        private readonly CancellationTokenSource _shutdown = new();
        private readonly LogoutHandler _logoutHandler = new();

        public TcpServer(int port, string connectionString = "")
        {
            if (port is < 1 or > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            _port = port;
            _listener = new TcpListener(IPAddress.Any, port);
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                _database = new DatabaseService(connectionString);
            }
        }

        public int ConnectedClientCount => _clients.Count;

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            using CancellationTokenRegistration registration = cancellationToken.Register(static state => ((TcpServer)state!).RequestStop(), this);
            _listener.Start();
            NhatKy.Ghi($"Server ?ã kh?i ??ng, l?ng nghe trên 0.0.0.0:{_port}.");

            try
            {
                await AcceptClientsAsync(_shutdown.Token);
            }
            catch (OperationCanceledException) when (_shutdown.IsCancellationRequested)
            {
                NhatKy.Ghi("Server nh?n yêu c?u d?ng.");
            }
            catch (SocketException) when (_shutdown.IsCancellationRequested)
            {
                NhatKy.Ghi("Server ?ã d?ng l?ng nghe.");
            }
            finally
            {
                await StopClientHandlersAsync();
                NhatKy.Ghi("Server ?ã d?ng.");
            }
        }

        public async Task StopAsync()
        {
            RequestStop();
            await StopClientHandlersAsync();
        }

        private async Task AcceptClientsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = await _listener.AcceptTcpClientAsync(cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (SocketException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                string clientId = Guid.NewGuid().ToString("N");
                client.NoDelay = true;
                _clients[clientId] = client;
                NhatKy.Ghi($"Client {clientId} ?ã k?t n?i t? {client.Client.RemoteEndPoint}.");

                Task handler = HandleClientAsync(clientId, client);
                _clientHandlers[clientId] = handler;
                _ = ObserveClientHandlerAsync(clientId, handler);
            }
        }

        private async Task HandleClientAsync(string clientId, TcpClient client)
        {
            try
            {
                using NetworkStream stream = client.GetStream();
                while (!_shutdown.IsCancellationRequested)
                {
                    ClientRequest? request = await MessageFraming.ReceiveAsync<ClientRequest>(stream, _shutdown.Token);
                    if (request is null)
                    {
                        NhatKy.Ghi($"Client {clientId} ?ã ?óng k?t n?i.");
                        break;
                    }

                    ServerResponse response = await ProcessRequestAsync(clientId, request, _shutdown.Token);
                    await MessageFraming.SendAsync(stream, response, _shutdown.Token);
                    if (request.Operation.Equals(ProtocolOperations.Logout, StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException) when (_shutdown.IsCancellationRequested)
            {
                NhatKy.Ghi($"Client {clientId} ?ang ???c ng?t k?t n?i do server d?ng.");
            }
            catch (IOException ex)
            {
                NhatKy.Loi($"K?t n?i client {clientId} b? gián ?o?n: {ex.Message}");
            }
            catch (SocketException ex)
            {
                NhatKy.Loi($"L?i socket c?a client {clientId}: {ex.Message}");
            }
            catch (ObjectDisposedException)
            {
                NhatKy.Ghi($"Socket c?a client {clientId} ?ã ???c ?óng.");
            }
            catch (Exception ex)
            {
                NhatKy.Loi($"L?i x? lý client {clientId}: {ex.Message}");
            }
            finally
            {
                _sessions.TryRemove(clientId, out _);
                _clients.TryRemove(clientId, out _);
                _clientHandlers.TryRemove(clientId, out _);
                try { client.Close(); } catch (ObjectDisposedException) { }
                NhatKy.Ghi($"Client {clientId} ?ã ???c d?n d?p sau khi phiên k?t thúc.");
            }
        }

        private async Task<ServerResponse> ProcessRequestAsync(string clientId, ClientRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Operation))
            {
                return Failure("INVALID_REQUEST", "Yêu c?u không h?p l?.");
            }

            try
            {
                return request.Operation.ToUpperInvariant() switch
                {
                    ProtocolOperations.Register => await RegisterAsync(request, cancellationToken),
                    ProtocolOperations.Login => await LoginAsync(clientId, request, cancellationToken),
                    ProtocolOperations.GetUserInfo => await GetUserInfoAsync(clientId, request, cancellationToken),
                    ProtocolOperations.Logout => Logout(clientId),
                    _ => Failure("UNKNOWN_OPERATION", "Thao tác không ???c h? tr?.")
                };
            }
            catch (DbException ex)
            {
                NhatKy.Loi($"L?i c? s? d? li?u khi x? lý {request.Operation}: {ex.Message}");
                return Failure("DATABASE_ERROR", "Không th? x? lý yêu c?u lúc này.");
            }
            catch (InvalidOperationException ex)
            {
                NhatKy.Loi($"L?i c?u hình khi x? lý {request.Operation}: {ex.Message}");
                return Failure("SERVER_CONFIGURATION_ERROR", "Server ch?a ???c c?u hình ??y ??.");
            }
        }

        private async Task<ServerResponse> RegisterAsync(ClientRequest request, CancellationToken cancellationToken)
        {
            if (_database is null)
            {
                return Failure("SERVER_CONFIGURATION_ERROR", "Server ch?a ???c c?u hình c? s? d? li?u.");
            }

            return await _database.RegisterAsync(request, cancellationToken);
        }

        private async Task<ServerResponse> LoginAsync(string clientId, ClientRequest request, CancellationToken cancellationToken)
        {
            if (_database is null)
            {
                return Failure("SERVER_CONFIGURATION_ERROR", "Server ch?a ???c c?u hình c? s? d? li?u.");
            }

            ServerResponse response = await _database.AuthenticateAsync(request, cancellationToken);
            if (response.Success && response.UserInfo is not null)
            {
                _sessions[clientId] = response.UserInfo;
                NhatKy.Ghi($"Client {clientId} ?ã ??ng nh?p v?i tài kho?n {response.UserInfo.Username}.");
            }

            return response;
        }

        private async Task<ServerResponse> GetUserInfoAsync(string clientId, ClientRequest request, CancellationToken cancellationToken)
        {
            if (!_sessions.TryGetValue(clientId, out UserDto? user))
            {
                return Failure("NOT_AUTHENTICATED", "Phiên ??ng nh?p không h?p l?.");
            }

            if (_database is null)
            {
                return new ServerResponse { Success = true, UserInfo = user };
            }

            return await _database.GetUserInfoAsync(request.UserId ?? user.UserId, cancellationToken);
        }

        private ServerResponse Logout(string clientId)
        {
            _sessions.TryRemove(clientId, out UserDto? user);
            if (_clients.TryGetValue(clientId, out TcpClient? client))
            {
                _logoutHandler.Handle(clientId, user?.Username, _clients);
            }
            else
            {
                NhatKy.Ghi($"Client {clientId} ?ã ??ng xu?t và ng?t k?t n?i.");
            }

            return new ServerResponse { Success = true };
        }

        private async Task ObserveClientHandlerAsync(string clientId, Task handler)
        {
            try { await handler; }
            catch (Exception ex) { NhatKy.Loi($"Handler c?a client {clientId} k?t thúc b?t th??ng: {ex.Message}"); }
        }

        private async Task StopClientHandlersAsync()
        {
            _shutdown.Cancel();
            _listener.Stop();
            foreach (TcpClient client in _clients.Values)
            {
                try { client.Close(); } catch (ObjectDisposedException) { }
            }

            Task[] handlers = _clientHandlers.Values.ToArray();
            if (handlers.Length > 0)
            {
                await Task.WhenAll(handlers);
            }
        }

        private void RequestStop()
        {
            _shutdown.Cancel();
            _listener.Stop();
        }

        private static ServerResponse Failure(string code, string message) => new() { ErrorCode = code, ErrorMessage = message };
    }
}
