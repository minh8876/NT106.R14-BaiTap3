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

The current repository contains pre-existing compilation errors in `Client\FormDangKy.cs` involving the missing `TruyCapDuLieu` and `MatKhau` classes. The server project builds independently.

## Run the server

Run with the default port `5000`:

```powershell
dotnet run --project Server\Server.csproj
```

Run with a custom port:

```powershell
dotnet run --project Server\Server.csproj -- 5001
```

The server listens on all network interfaces (`0.0.0.0`). Press `Ctrl+C` to stop it gracefully.

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

## Current protocol note

The server supports the existing newline-delimited `DANGXUAT` command. The current login flow still contains simulated/incomplete communication, so a complete client login-to-server workflow requires the missing login protocol implementation.
