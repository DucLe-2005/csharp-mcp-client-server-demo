using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Client;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using System.Linq;
using System.Text.Json;
using System.Reflection.Metadata.Ecma335;
using Azure;
using Azure.AI.Inference;
using Azure.Identity;

// ---- Azure model client ----
var endpoint = "https://models.inference.ai.azure.com";
var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
if (string.IsNullOrEmpty(token))
{
    Console.WriteLine("Please set the GITHUB_TOKEN environment variable.");
    return;
}
var client = new ChatCompletionsClient(new Uri(endpoint), new AzureKeyCredential(token));
var chatHistory = new List<ChatRequestMessage>
{
    new ChatRequestSystemMessage("You are a helpful assistant that knows about AI")
};

// ---- MCP client & transport ----
var clientTransport = new HttpClientTransport(new() { Endpoint = new Uri("http://localhost:5178") });
await using var mcpClient = await McpClient.CreateAsync(clientTransport);

// ---- Notification handlers ----


// helpers
static string GetString(JsonElement obj, string name, string fallback = "")
    => obj.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.Null ? el.ToString() : fallback;

static double? GetDouble(JsonElement obj, string name)
    => obj.TryGetProperty(name, out var el) && el.TryGetDouble(out var d) ? d : null;


//await using var progressHandler = mcpClient.RegisterNotificationHandler(
//    NotificationMethods.ProgressNotification,
//    async (JsonRpcNotification n, CancellationToken ct) =>
//    {
//        var p = JsonSerializer.Deserialize<ProgressNotificationParams>(
//            n.Params,
//            ModelContextProtocol.McpJsonUtilities.DefaultOptions);

//        Console.WriteLine($"[Progress] {p?.Progress?.Message}  " +
//                  $"{(p?.Progress?.Total is null ? "" : $"({p.Progress.Progress}/{p.Progress.Total})")}");
//        await ValueTask.CompletedTask;
//    });

//var clientOptions = new Client

static JsonElement AsElement(object? p)
    => p is JsonElement je
        ? je
        : JsonSerializer.SerializeToElement(p, ModelContextProtocol.McpJsonUtilities.DefaultOptions);

await using var anySub = mcpClient.RegisterNotificationHandler(
    "*",  // handles every notification method
    async (n, ct) =>
    {
        Console.WriteLine($"[ANY] method={n.Method} params={JsonSerializer.Serialize(n.Params)}");
        await ValueTask.CompletedTask;
    });


await using var msgSub = mcpClient.RegisterNotificationHandler(
    "notifications/message",
    async (n, ct) =>
    {
        var root = AsElement(n.Params); // your helper

        // level & logger with safe defaults
        string level = root.TryGetProperty("level", out var l) ? (l.GetString() ?? "info") : "info";
        string logger = root.TryGetProperty("logger", out var lg) ? (lg.GetString() ?? "server") : "server";


        string text;
        if (root.TryGetProperty("message", out var m) && m.ValueKind != JsonValueKind.Null)
        {
            text = m.GetString() ?? "";
        }
        else if (root.TryGetProperty("data", out var d))
        {
            text = d.ValueKind == JsonValueKind.Object && d.TryGetProperty("text", out var t)
                   ? (t.GetString() ?? d.ToString())
                   : d.ToString();
        }
        else
        {
            text = root.ToString(); // last-resort: print entire params
        }

        Console.WriteLine($"[MSG:{level}] ({logger}) {text}");
        await ValueTask.CompletedTask;
    });


ChatCompletionsToolDefinition ConvertFrom(string name, string description, JsonElement jsonElement)
{
    // convert the tool to a function definition
    FunctionDefinition functionDefinition = new FunctionDefinition(name)
    {
        Description = description,
        Parameters = BinaryData.FromObjectAsJson(new
        {
            Type = "object",
            Properties = jsonElement
        },
        new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
    };

    // create a tool definition
    ChatCompletionsToolDefinition toolDefinition = new ChatCompletionsToolDefinition(functionDefinition);
    return toolDefinition;
}

async Task<List<ChatCompletionsToolDefinition>> GetMcpTools()
{
    Console.WriteLine("Listing tools");
    var tools = await mcpClient.ListToolsAsync();

    List<ChatCompletionsToolDefinition> toolDefinitions = new List<ChatCompletionsToolDefinition>();

    foreach (var tool in tools)
    {
        Console.WriteLine($"Connected to server with tools: {tool.Name}");
        Console.WriteLine($"Tool description: {tool.Description}");
        Console.WriteLine($"Tool parameters: {tool.JsonSchema}");

        JsonElement propertiesElement;
        tool.JsonSchema.TryGetProperty("properties", out propertiesElement);

        var def = ConvertFrom(tool.Name, tool.Description, propertiesElement);
        Console.WriteLine($"Tool definition: {def}");
        toolDefinitions.Add(def);

        Console.WriteLine($"Properties: {propertiesElement}");
    }

    return toolDefinitions;
}

// list tools on mcp server
var tools = await GetMcpTools();
for (int i = 0; i < tools.Count; i++)
{
    var tool = tools[i];
    Console.WriteLine($"MCP Tools def: {i}: {tool}");
}

// define the chat history and the user message
var userMessage = "say hello with a greeting message";

chatHistory.Add(new ChatRequestUserMessage(userMessage));

// define options, including the tools
var options = new ChatCompletionsOptions(chatHistory)
{
    Model = "gpt-4o-mini",
    Tools = { tools[0] }
};

// call the model
ChatCompletions? response = await client.CompleteAsync(options);
var content = response.Content;

// check if the response contains a function call
ChatCompletionsToolCall? calls = response.ToolCalls.FirstOrDefault();
for (int i = 0; i < response.ToolCalls.Count; i++)
{
    var call = response.ToolCalls[i];
    Console.WriteLine($"Tool call {i}: {call.Arguments}");

    var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(call.Arguments);
    CallToolResult result = await mcpClient.CallToolAsync(
        call.Name,
        dict!,
        cancellationToken: CancellationToken.None);

    PrintToolResult(result);
}
;

static void PrintToolResult(CallToolResult res)
{
    var textBlock = res.Content.FirstOrDefault(b => string.Equals(b.Type, "text", StringComparison.OrdinalIgnoreCase));

    if (textBlock != null)
    {
        var textProp = textBlock.GetType().GetProperty("Text");  // some preview versions expose this
        if (textProp != null)
        {
            Console.WriteLine(textProp.GetValue(textBlock)?.ToString());
        }
        else
        {
            Console.WriteLine(JsonSerializer.Serialize(textBlock, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
    else
    {
        Console.WriteLine(JsonSerializer.Serialize(res, new JsonSerializerOptions { WriteIndented = true }));
    }
};