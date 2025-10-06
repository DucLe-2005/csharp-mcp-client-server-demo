using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();
await builder.Build().RunAsync();

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