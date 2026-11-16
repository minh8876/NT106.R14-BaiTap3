namespace Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            NhatKy.KhoiTao();
            using CancellationTokenSource cancellation = new();

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cancellation.Cancel();
            };

            ServerSettings settings = ServerSettings.FromEnvironment(args);
            var server = new TcpServer(settings.Port, settings.ConnectionString);
            await server.StartAsync(cancellation.Token);
            await server.StopAsync();
        }
    }
}
