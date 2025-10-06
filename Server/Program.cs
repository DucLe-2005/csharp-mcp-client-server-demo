using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
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




[McpServerTool("A tool that sends progress notification")]
public async Task<TextContent> ProcessFiles(
    IProgress<ProgressNotificationValue> progress,
    string message,
    CancellationToken cancellationToken)
{
    progress.Report(new ProgressNotificationValue("Processing file 1/3..."));
    progress.Report(new ProgressNotificationValue("Processing file 2/3..."));
    progress.Report(new ProgressNotificationValue("Processing file 3/3..."));

    var result = new CallToolResult
    {
        Content = new[]
        {
            new TextContentBlock
            {
                Type = "text",
                Text = $"Done: {message}"
            }
        }
    };

    return Task.FromResult(result);
}

[McpServerToolType]
public static class McpCalculatorServer
{
    [McpServerTool, Description("Calculates the sum of two numbers")]
    public static double Add(double numberA, double numberB)
    {
        return numberA + numberB;
    }

    //[McpServerTool, Description("Calculates the difference of two numbers")]
    //public static double Subtract(double numberA, double numberB)
    //{
    //    return numberA - numberB;
    //}

    //[McpServerTool, Description("Calculates the product of two numbers")]
    //public static double Multiply(double numberA, double numberB)
    //{
    //    return numberA * numberB;
    //}

    //[McpServerTool, Description("Calculates the quotient of two numbers")]
    //public static double Divide(double numberA, double numberB)
    //{
    //    if (numberB == 0)
    //    {
    //        throw new ArgumentException("Cannot divide by zero");
    //    }
    //    return numberA / numberB;
    //}

    //[McpServerTool, Description("Validates if a number is prime")]
    //public static bool IsPrime(long number)
    //{
    //    if (number <= 1) return false;
    //    if (number <= 3) return true;
    //    if (number % 2 == 0 || number % 3 == 0) return false;

    //    // Check divisibility using the 6k±1 optimization
    //    for (long i = 5; i * i <= number; i += 6)
    //    {
    //        if (number % i == 0 || number % (i + 2) == 0)
    //        {
    //            return false;
    //        }
    //    }

    //    return true;
    //}
}