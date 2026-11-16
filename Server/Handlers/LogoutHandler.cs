using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Server.Handlers
{
    public class LogoutHandler
    {
        public void Handle(string clientId, string username, ConcurrentDictionary<string, TcpClient> danhSachClient)
        {
            if (danhSachClient != null && danhSachClient.TryRemove(clientId, out TcpClient client))
            {
                try { client.Close(); } catch { }
            }

            string tenHienThi = string.IsNullOrWhiteSpace(username) ? clientId : $"{username} ({clientId})";
            NhatKy.Ghi($"Client {tenHienThi} đã đăng xuất và ngắt kết nối.");
        }
    }
}
