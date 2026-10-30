using System.ComponentModel;
using System.Diagnostics;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

if (args is ["--help"] or ["-h"])
{
    Console.WriteLine("Start: dotnet run --project tools/LocalLauncher");
    Console.WriteLine("Stop:  dotnet run --project tools/LocalLauncher -- stop");
    Console.WriteLine("Ctrl+C also stops the API. PostgreSQL retains its data and keeps running.");
    return 0;
}

using var shutdown = new CancellationTokenSource();
NamedPipeServerStream? control = null;
Task? controlTask = null;
ConsoleCancelEventHandler cancelHandler = (_, e) =>
{
    e.Cancel = true;
    shutdown.Cancel();
};

try
{
    var root = FindRepositoryRoot();
    var identity = OperatingSystem.IsWindows() ? root.ToUpperInvariant() : root;
    var pipeName = "perry-chat-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))[..16];

    if (args is ["stop"] or ["--stop"])
        return await RequestStop(pipeName);
    if (args.Length != 0)
    {
        Console.Error.WriteLine("Unknown arguments. Use --help for usage.");
        return 2;
    }

    try
    {
        control = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1,
            PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
    }
    catch (IOException)
    {
        Console.Error.WriteLine("A launcher is already running. Stop it with: dotnet run --project tools/LocalLauncher -- stop");
        return 1;
    }
    controlTask = WatchStop(control, shutdown);
    Console.CancelKeyPress += cancelHandler;

    if (!PortAvailable())
    {
        Console.Error.WriteLine("Port 7000 is already in use. Stop the previous API in its original terminal before starting another copy.");
        return 1;
    }

    Console.WriteLine("Starting local PostgreSQL...");
    var databaseExit = await Run(Command("docker", root, "compose", "-f", "compose.chat-local.yml", "up", "-d", "--wait"), shutdown.Token);
    if (databaseExit != 0)
    {
        Console.Error.WriteLine("PostgreSQL did not start. Check the Docker output above.");
        return databaseExit;
    }

    var projectDirectory = Path.Combine(root, "src", "Perry.Api");
    var project = Path.Combine(projectDirectory, "Perry.Api.csproj");
    var buildExit = await Run(Command("dotnet", root, "build", project, "--configuration", "Debug", "--nologo"), shutdown.Token);
    if (buildExit != 0)
        return buildExit;

    // Own the API directly: a 'dotnet run' wrapper can exit before its child.
    var api = Command("dotnet", projectDirectory, Path.Combine(projectDirectory, "bin", "Debug", "net10.0", "Perry.Api.dll"));
    using (var settings = JsonDocument.Parse(File.ReadAllText(Path.Combine(projectDirectory, "Properties", "launchSettings.json"))))
    {
        foreach (var variable in settings.RootElement.GetProperty("profiles").GetProperty("ChatLocal").GetProperty("environmentVariables").EnumerateObject())
            api.Environment[variable.Name] = variable.Value.GetString();
    }
    api.Environment["DOTNET_ENVIRONMENT"] = "Development";
    Console.WriteLine("Starting the API. Open http://localhost:7000/chat-local once ready.");
    return await Run(api, shutdown.Token);
}
catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
{
    Console.WriteLine("Local API stopped. PostgreSQL is unchanged.");
    return 0;
}
catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or IOException or JsonException)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}
finally
{
    Console.CancelKeyPress -= cancelHandler;
    shutdown.Cancel();
    // Closing the pipe acknowledges stop after child cleanup has finished.
    try
    {
        if (controlTask is not null)
            await controlTask;
    }
    finally { control?.Dispose(); }
}

static string FindRepositoryRoot()
{
    foreach (var start in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
    {
        for (var directory = new DirectoryInfo(start); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "compose.chat-local.yml")) &&
                File.Exists(Path.Combine(directory.FullName, "src", "Perry.Api", "Perry.Api.csproj")))
                return directory.FullName;
        }
    }
    throw new InvalidOperationException("Could not find the repository containing compose.chat-local.yml and Perry.Api.");
}

static ProcessStartInfo Command(string executable, string workingDirectory, params string[] arguments)
{
    var start = new ProcessStartInfo(executable) { WorkingDirectory = workingDirectory, UseShellExecute = false };
    foreach (var argument in arguments)
        start.ArgumentList.Add(argument);
    return start;
}

static async Task<int> Run(ProcessStartInfo start, CancellationToken ct)
{
    ct.ThrowIfCancellationRequested();
    using var process = new Process { StartInfo = start };
    if (!process.Start())
        throw new InvalidOperationException($"Could not start {start.FileName}.");
    try
    {
        await process.WaitForExitAsync(ct);
        ct.ThrowIfCancellationRequested();
        return process.ExitCode;
    }
    finally
    {
        // If the child did not receive Ctrl+C, terminate our own child after a grace period.
        if (!process.HasExited)
        {
            using var grace = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            try { await process.WaitForExitAsync(grace.Token); }
            catch (OperationCanceledException)
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync(CancellationToken.None);
            }
        }
    }
}

static bool PortAvailable()
{
    var probe = new TcpListener(IPAddress.Loopback, 7000);
    try { probe.Start(); return true; }
    catch (SocketException) { return false; }
    finally { probe.Stop(); }
}

static async Task WatchStop(NamedPipeServerStream pipe, CancellationTokenSource shutdown)
{
    try
    {
        while (!shutdown.IsCancellationRequested)
        {
            await pipe.WaitForConnectionAsync(shutdown.Token);
            using var readTimeout = CancellationTokenSource.CreateLinkedTokenSource(shutdown.Token);
            readTimeout.CancelAfter(TimeSpan.FromSeconds(3));
            var command = new byte[4];
            try
            {
                await pipe.ReadExactlyAsync(command, readTimeout.Token);
                if (command.AsSpan().SequenceEqual("stop"u8))
                {
                    Console.WriteLine("Stop requested from another terminal...");
                    shutdown.Cancel();
                    return;
                }
            }
            catch (OperationCanceledException) when (!shutdown.IsCancellationRequested) { }
            catch (IOException) { }
            pipe.Disconnect();
        }
    }
    catch (OperationCanceledException) when (shutdown.IsCancellationRequested) { }
    catch (IOException)
    {
        shutdown.Cancel();
    }
}

static async Task<int> RequestStop(string pipeName)
{
    using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut,
        PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
    try
    {
        await pipe.ConnectAsync(1500);
        await pipe.WriteAsync("stop"u8.ToArray());
        await pipe.FlushAsync();
        Console.WriteLine("Stopping the local API...");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        if (await pipe.ReadAsync(new byte[1], timeout.Token) != 0)
            throw new IOException("Unexpected response from the local launcher.");
        if (!PortAvailable())
        {
            Console.Error.WriteLine("The launcher stopped, but port 7000 is still occupied by another process.");
            return 1;
        }
        Console.WriteLine("Local API stopped. Port 7000 is free.");
        return 0;
    }
    catch (TimeoutException)
    {
        Console.Error.WriteLine("No launcher was found for this repository. An API started separately must be stopped in its own terminal.");
        return 1;
    }
    catch (OperationCanceledException)
    {
        Console.Error.WriteLine("The API did not stop within 15 seconds.");
        return 1;
    }
}
