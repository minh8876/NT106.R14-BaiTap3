# NT106.R14-BaiTap3

C# TCP client/server project targeting .NET 9.

## Prerequisites

- Windows
- .NET 9 SDK
- SQL Server configured for the server database features
- Windows Firewall access for the server port when using another computer

Check the installed SDK:

```powershell
dotnet --version
```

## Build

Build the server:

```powershell
dotnet build Server\Server.csproj
```

Build the client:

```powershell
dotnet build Client\Client.csproj
```

Build all projects:

```powershell
dotnet build
```

The solution currently builds successfully. Existing nullable-reference and SQL client deprecation warnings may remain.

## Run the server

Configure the SQL Server connection string without committing credentials:

```powershell
$env:NT106_CONNECTION_STRING = "your-local-connection-string"
```

Run with the default port `5000`:

```powershell
dotnet run --project Server\Server.csproj
```

Run with a custom port:

```powershell
dotnet run --project Server\Server.csproj -- 5001
```

The server listens on all network interfaces (`0.0.0.0`). Press `Ctrl+C` to stop it gracefully.

### UTF-8 console output

If Vietnamese log messages appear as `?` or `ã`, configure the PowerShell console as UTF-8 before starting the server:

```powershell
chcp 65001
[Console]::InputEncoding = [System.Text.UTF8Encoding]::new($false)
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$OutputEncoding = [System.Text.UTF8Encoding]::new($false)
```

The active console code page should report `65001`:

```powershell
chcp
```

## Run the client

Start the Windows Forms client with:

```powershell
dotnet run --project Client\Client.csproj
```

Enter the server computer's IP address and the same port configured for the server. Use `127.0.0.1` when the server and client run on the same computer.

For a different computer on the LAN, use the server's local IP address, for example:

```text
192.168.1.25
```

Allow inbound TCP traffic for the selected port in Windows Firewall.

## Project structure

- `Server` - TCP server, client session management, logging, and database handlers
- `Client` - Windows Forms client
- `SharedLibrary` - Shared login and user data models

## Protocol

The Client and Server communicate using UTF-8 JSON messages with a 4-byte length prefix. Supported operations are:

- `DANGKY` - register an account
- `DANGNHAP` - authenticate a user
- `LAYTHONGTIN` - retrieve authenticated user information
- `DANGXUAT` - log out and close the session

All database operations run on the Server. The Client never connects directly to SQL Server.
