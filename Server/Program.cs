using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddMcpServer()
     //.WithStdioServerTransport()
    .WithHttpTransport()
    .WithToolsFromAssembly();

builder.WebHost.UseUrls("http://localhost:5178");

var app = builder.Build();
app.MapMcp();
app.Run();


[McpServerToolType]
public static class McpBasicTool
{
    [McpServerTool, Description("Calculates the sum of two numbers")]
    public static double Add(double numberA, double numberB)
    {
        return numberA + numberB;
    }

    [McpServerTool, Description("Says hello to the world with progress updates")]
    public static async Task<string> SayHello(
        IProgress<ProgressNotificationValue> progress, // progress channel
        McpServer server,
        CancellationToken ct)
    {
        await server.SendNotificationAsync(
            "notifications/message",
            new { level = "info", logger = "demo", message = "Starting..."  },
            cancellationToken: ct);

        await server.SendNotificationAsync(
            "notifications/message",
            new { level = "info", logger = "demo", message = "All done"  },
            cancellationToken: ct);

        return "Finished";
    }
}
