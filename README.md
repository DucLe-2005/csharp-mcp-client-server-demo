upda# 🧩 MCP Client–Server Demo (.NET)

This project is a **simple implementation of a Model Context Protocol (MCP)** system using **.NET**, consisting of two components:

- **Server** — a minimal MCP server exposing tools via stdio transport  
- **Client** — a .NET console client that connects to the server and interacts with an **LLM** through the MCP protocol

This demo follows [Microsoft’s *Model Context Protocol for Beginners*](https://github.com/microsoft/mcp-for-beginners) and is meant as a learning project for understanding how MCP clients and servers communicate in .NET.

---

## 🧠 Overview

The **Model Context Protocol (MCP)** defines how AI models and tools can exchange structured messages over a standardized transport layer (like `stdio` or `SSE`).  
This demo helps you understand:
- How an MCP server exposes tools using `.NET` and dependency injection  
- How an MCP client connects and interacts with the model  
- How messages are exchanged via JSON-RPC and serialized in the MCP format  

### 📁 Project Structure
```
mcp-dotnet-demo/
│
├── Client/        # The MCP client that sends requests and interacts with the LLM
└── Server/        # The MCP server that exposes tools and handles requests
```

---

## ⚙️ Getting Started

### 1️⃣ Clone this repository
```bash
git clone https://github.com/<your-username>/mcp-dotnet-demo.git
cd mcp-dotnet-demo
```

---

### 2️⃣ Install prerequisites

Make sure you have the following installed:

- [.NET SDK 8.0+](https://dotnet.microsoft.com/en-us/download)
- (Optional) [Node.js](https://nodejs.org/) if you want to use the MCP Inspector (`npx @modelcontextprotocol/inspector`)

---

### 3️⃣ Build and run the **Server**
```bash
cd Server
dotnet build
dotnet run
```

The server will start and listen via **stdio** transport.  
It exposes basic MCP tools defined in your C# project.

---

### 4️⃣ Build and run the **Client**
In another terminal:
```bash
cd Client
dotnet build
dotnet run
```

The client will connect to the MCP server and send messages to interact with the LLM.  
You should see structured request/response messages printed in the console.

---

### 5️⃣ (Optional) Inspect with MCP Inspector
You can visualize the client-server interaction using the [MCP Inspector tool](https://www.npmjs.com/package/@modelcontextprotocol/inspector):

```bash
npx @modelcontextprotocol/inspector dotnet run --project ./Server
```

---

## 🧰 Technologies Used

- **C# / .NET 8**
- **Microsoft.Extensions.Hosting**
- **ModelContextProtocol SDK**
- **JSON-RPC / stdio Transport**

---

## 📚 References

- Microsoft’s official guide: [Model Context Protocol for Beginners](https://github.com/microsoft/mcp-for-beginners)
- [MCP's C# SDK](https://github.com/modelcontextprotocol/csharp-sdk?utm_source=chatgpt.com)

---

## 🪄 License

This project is provided for educational and demo purposes.  
See the [MCP for Beginners License](https://github.com/microsoft/mcp-for-beginners/blob/main/LICENSE) for reference.
