namespace Server
{
    public sealed class ServerSettings
    {
        public int Port { get; init; } = 5000;
        public string ConnectionString { get; init; } = string.Empty;

        public static ServerSettings FromEnvironment(string[] args)
        {
            int port = 5000;
            if (args.Length > 0 && int.TryParse(args[0], out int argumentPort))
            {
                port = argumentPort;
            }

            string connectionString = Environment.GetEnvironmentVariable("NT106_CONNECTION_STRING") ?? string.Empty;
            return new ServerSettings { Port = port, ConnectionString = connectionString };
        }
    }
}
