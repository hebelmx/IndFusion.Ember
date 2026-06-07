# 🔥 IndFusion.Ember

**The fusion point where real-time messages ignite and spread across any transport.**

[![NuGet](https://img.shields.io/nuget/v/IndFusion.Ember.svg)](https://www.nuget.org/packages/IndFusion.Ember/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---

## 🌟 What is Ember?

IndFusion.Ember is a **transport-agnostic real-time communication abstraction** that follows Clean Architecture principles. Think of it as a glowing ember spreading through your system:

- 🔥 **Messages Ignite** - Events and data start flowing
- 📡 **Propagate & Track** - Health monitoring and metrics
- 💡 **Illuminate & Display** - Real-time dashboards and UI updates

Like an ember that spreads warmth and light, Ember spreads real-time updates across any transport layer without coupling your application to a specific technology.

---

## ⚡ Three Actors Pattern

Every Ember implementation follows the **Three Actors Pattern**:

| Actor | Essence | Purpose |
|-------|---------|---------|
| **🚀 ExxerHub&lt;T&gt;** | Something Moving | Messages, events, data flows |
| **🩺 ServiceHealth&lt;T&gt;** | Something Tracking | Health metrics, status monitoring |
| **📊 Dashboard&lt;T&gt;** | Something Displaying | UI dashboards, real-time visualization |

---

## 🚀 Quick Start

### Installation

```bash
dotnet add package IndFusion.Ember
```

### Basic Usage

```csharp
// 1. Register Ember services
services.AddSignalRAbstractions();

// 2. Create your hub (something moving)
public class MyHub : ExxerHub<MyData>
{
    public MyHub(ILogger<MyHub> logger) : base(logger) { }
}

// 3. Track health (something tracking)
public class MyService
{
    private readonly IServiceHealth<MyService> _health;

    public MyService(IServiceHealth<MyService> health)
    {
        _health = health;
        await _health.UpdateHealthAsync(HealthStatus.Healthy);
    }
}

// 4. Display dashboard (something displaying)
public class MyDashboard : Dashboard<MyData>
{
    public MyDashboard(ILogger<MyDashboard> logger) : base(logger) { }
}
```

---

## 🔌 Transport Fusion

**Currently Implemented:**
- ✅ SignalR (WebSockets, Server-Sent Events, Long Polling)

**Designed For Universal Fusion:**
- 🔄 gRPC Streams
- 🌐 Raw WebSockets
- 🔌 Raw TCP/UDP
- ☁️ Azure Service Bus
- ☁️ AWS EventBridge
- 🏭 OPC-UA (Industrial IoT)
- 📡 MQTT
- 🐰 RabbitMQ

**The abstraction layer allows easy transport swapping without changing application code.**

---

## 🏗️ Clean Architecture

Ember follows **Hexagonal Architecture** principles:

```
┌─────────────────────────────────────────┐
│         Your Application Core           │
│    (Domain Logic, Use Cases)            │
└─────────────────┬───────────────────────┘
                  │
         ┌────────▼────────┐
         │  IndFusion.Ember │  ← Abstraction Layer
         │   (Interfaces)   │
         └────────┬─────────┘
                  │
    ┌─────────────┼─────────────┐
    │             │             │
┌───▼────┐   ┌───▼────┐   ┌───▼────┐
│SignalR │   │ gRPC   │   │ Azure  │  ← Plug Any Transport
└────────┘   └────────┘   └────────┘
```

**Benefits:**
- 🎯 **Testable**: Mock IExxerHub, IServiceHealth, IDashboard
- 🔄 **Swappable**: Change transports without touching business logic
- 📦 **Portable**: Use across microservices, monoliths, serverless
- 🛡️ **Railway-Oriented**: Built-in Result&lt;T&gt; pattern for error handling

---

## 💎 Railway-Oriented Programming

All operations return `Result<T>` for elegant error handling:

```csharp
var result = await hub.SendToAllAsync(data, cancellationToken);

if (result.IsSuccess)
{
    // Success path
    logger.LogInformation("Message sent successfully");
}
else
{
    // Failure path - no exceptions thrown
    logger.LogWarning("Failed to send: {Error}", result.Error);
}
```

**No exceptions during normal flow** - defensive intelligence built-in.

---

## 🎯 Use Cases

Perfect for:

- 📊 **Real-time Dashboards** - Stock tickers, IoT sensors, metrics
- 🩺 **Health Monitoring** - Service status, heartbeat tracking
- 🔔 **Notifications** - User alerts, system events
- 📈 **Live Analytics** - Real-time charts, KPI updates
- 🎮 **Multiplayer Games** - Player synchronization
- 💬 **Chat Applications** - Real-time messaging
- 🏭 **Industrial IoT** - Factory automation, sensor networks

---

## 📚 Documentation

- [Getting Started Guide](https://github.com/hebelmx/IndFusion.Ember/wiki/Getting-Started)
- [API Reference](https://github.com/hebelmx/IndFusion.Ember/wiki/API-Reference)
- [Three Actors Pattern](https://github.com/hebelmx/IndFusion.Ember/wiki/Three-Actors-Pattern)
- [Transport Swapping](https://github.com/hebelmx/IndFusion.Ember/wiki/Transport-Swapping)

---

## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md).

---

## 📄 License

MIT License - see [LICENSE](LICENSE) for details.

---

## 🔥 About the Name

**Ember** represents a glowing piece of fire that spreads warmth and light - just like how real-time messages spread through your distributed system, illuminating dashboards and bringing life to UI components.

**IndFusion** signifies the **Industrial Fusion** of different transport technologies into a unified abstraction.

---

## 💬 Support

- **Issues**: [GitHub Issues](https://github.com/hebelmx/IndFusion.Ember/issues)
- **Discussions**: [GitHub Discussions](https://github.com/hebelmx/IndFusion.Ember/discussions)
- **Author**: Abel Briones Ramirez
- **Company**: Exxerpro Solutions
- **Email**: abel.briones@exxerpro.com

---

<div align="center">

**🔥 Let the ember ignite your real-time architecture 🔥**

Made with ❤️ by [Exxerpro Solutions](https://exxerpro.com)

</div>
